using Sandbox;
using System;

namespace Murder;

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

		DropCarriable();
		DeleteFlashlight();

		Event.Run( GameEvent.Player.Killed, this );
		GameManager.Instance.State.OnPlayerKilled( this );

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
			if ( GameManager.Instance.State is not GameplayState and not PostRound )
				return;
		}

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
