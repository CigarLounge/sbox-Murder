using Sandbox;
using System;

namespace Murder;

public partial class Player
{
	[Net] public Player Killer { get; private set; }
	[Net] public TimeSince TimeSinceDeath { get; private set; }
	/// <summary>
	/// When a player teamkills, he will be blinded until he is clean.
	/// </summary>
	[Net] public TimeUntil TimeUntilClean { get; private set; }
	public DamageInfo LastDamage { get; private set; }

	public override void OnKilled()
	{
		Game.AssertServer();

		LifeState = LifeState.Dead;
		TimeSinceDeath = 0;

		if ( LastAttacker is Player killer )
		{
			Killer = killer;

			if ( killer.Role == Role )
			{
				killer.DropCarriable();
				killer.TimeUntilClean = 20f;
			}
			else if ( killer.Role == Role.Murderer )
				killer.TimeUntilClean = -0.1f;
		}

		Controller = null;
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
		Game.AssertClient();

		if ( IsLocalPawn )
		{
			if ( Corpse.IsValid() )
				CameraMode.Current = new DeathCamera();
		}

		DeleteFlashlight();
		Event.Run( GameEvent.Player.Killed, this );
	}

	public override void TakeDamage( DamageInfo info )
	{
		Game.AssertServer();

		if ( !this.IsAlive() )
			return;

		if ( info.HasTag( "blast" ) )
			Deafen( To.Single( this ), info.Damage.LerpInverse( 0, 60 ) );

		CreateBloodSplatter( info, 180f );

		info.Damage = Math.Min( Health, info.Damage );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		LastDamage = info;

		Health -= info.Damage;

		this.ProceduralHitReaction( info );

		if ( Health <= 0f )
			OnKilled();
	}

	private void CreateBloodSplatter( DamageInfo info, float maxDistance )
	{
		var tr = Trace.Ray( new Ray( info.Position, info.Force.Normal ), maxDistance )
			.Ignore( this )
			.Run();

		if ( !tr.Hit )
			return;

		var decal = ResourceLibrary.Get<DecalDefinition>( "decals/blood_splatter.decal" );
		Decal.Place( To.Everyone, decal, null, 0, tr.EndPosition - tr.Direction * 1f, Rotation.LookAt( tr.Normal ), Color.White );
	}

	private void ResetDamageData()
	{
		LastAttacker = null;
		LastAttackerWeapon = null;
		LastDamage = default;
		Killer = null;
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
