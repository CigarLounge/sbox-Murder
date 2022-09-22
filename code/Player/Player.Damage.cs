using Sandbox;
using System;

namespace Murder;

public struct ColorGroup
{
	public Color Color;
	public string Title;

	public ColorGroup( string title, Color color )
	{
		Title = title;
		Color = color;
	}
}

public enum HitboxGroup
{
	None = -1,
	Generic = 0,
	Head = 1,
	Chest = 2,
	Stomach = 3,
	LeftArm = 4,
	RightArm = 5,
	LeftLeg = 6,
	RightLeg = 7,
	Gear = 10,
	Special = 11,
}

public partial class Player
{
	public const float MaxHealth = 100f;

	[Net]
	public TimeSince TimeSinceDeath { get; private set; }

	[Net]
	public TimeUntil TimeUntilClean { get; private set; }

	public DamageInfo LastDamage { get; private set; }

	public new float Health
	{
		get => base.Health;
		set => base.Health = Math.Clamp( value, 0, MaxHealth );
	}

	private static readonly ColorGroup[] _healthGroupList = new ColorGroup[]
	{
		new ColorGroup("Near Death", Color.FromBytes(246, 6, 6)),
		new ColorGroup("Badly Wounded", Color.FromBytes(234, 129, 4)),
		new ColorGroup("Wounded", Color.FromBytes(213, 202, 4)),
		new ColorGroup("Hurt", Color.FromBytes(171, 231, 3)),
		new ColorGroup("Healthy", Color.FromBytes(44, 233, 44))
	};

	public ColorGroup GetHealthGroup( float health )
	{
		if ( Health > MaxHealth )
			return _healthGroupList[^1];

		var index = (int)((health - 1f) / (MaxHealth / _healthGroupList.Length));
		return _healthGroupList[index];
	}

	public override void OnKilled()
	{
		Host.AssertServer();

		LifeState = LifeState.Dead;
		TimeSinceDeath = 0;
		Client.AddInt( "deaths" );

		if ( LastAttacker is Player player && Role == player.Role )
		{
			player.DropCarriable();
			player.TimeUntilClean = 20f;
			player.Client.AddInt( "kills" );
		}

		Corpse = new Corpse( this );
		RemoveAllDecals();
		StopUsing();

		EnableAllCollisions = false;
		EnableDrawing = false;
		EnableTouch = false;

		SetCarriable( null );
		DeleteFlashlight();

		Event.Run( GameEvent.Player.Killed, this );
		Game.Current.State.OnPlayerKilled( this );

		ClientOnKilled( this );
	}

	private void ClientOnKilled()
	{
		Host.AssertClient();

		if ( IsLocalPawn )
			CurrentChannel = Channel.Spectator;

		DeleteFlashlight();
		Event.Run( GameEvent.Player.Killed, this );
	}

	public override void TakeDamage( DamageInfo info )
	{
		Host.AssertServer();

		if ( !this.IsAlive() )
			return;

		if ( info.Attacker is Player attacker && attacker != this )
		{
			if ( Game.Current.State is not GameplayState and not PostRound )
				return;
		}

		if ( info.Flags.HasFlag( DamageFlags.Bullet ) )
			info.Damage *= GetBulletDamageMultipliers( ref info );

		if ( info.Flags.HasFlag( DamageFlags.Blast ) )
			Deafen( To.Single( this ), info.Damage.LerpInverse( 0, 60 ) );

		info.Damage = Math.Min( Health, info.Damage );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		LastDamage = info;

		Health -= info.Damage;
		Event.Run( GameEvent.Player.TookDamage, this );

		this.ProceduralHitReaction( info );

		if ( Health <= 0f )
			OnKilled();
	}

	private float GetBulletDamageMultipliers( ref DamageInfo info )
	{
		var damageMultiplier = 1f;

		var hitboxGroup = (HitboxGroup)GetHitboxGroup( info.HitboxIndex );

		if ( hitboxGroup == HitboxGroup.Head )
			damageMultiplier *= 2f;
		else if ( hitboxGroup >= HitboxGroup.LeftArm && hitboxGroup <= HitboxGroup.Gear )
			damageMultiplier *= 0.55f;

		return damageMultiplier;
	}

	private void ResetDamageData()
	{
		LastAttacker = null;
		LastAttackerWeapon = null;
		LastDamage = default;
	}

	[ClientRpc]
	public static void Deafen( float strength )
	{
		Audio.SetEffect( "flashbang", strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	[ClientRpc]
	public static void ClientOnKilled( Player player )
	{
		if ( !player.IsValid() )
			return;

		player.ClientOnKilled();
	}
}
