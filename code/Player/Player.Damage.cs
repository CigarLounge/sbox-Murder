using Sandbox;
using System;

namespace Murder;

public partial class Player
{
	[Net] public TimeSince TimeSinceDeath { get; private set; }
	/// <summary>
	/// This gets set when a player teamkills.
	/// </summary>
	[Net] public TimeUntil TimeUntilClean { get; private set; }
	public DamageInfo LastDamage { get; private set; }

	public override void OnKilled()
	{
		Game.AssertServer();

		LifeState = LifeState.Dead;
		TimeSinceDeath = 0;

		if ( LastAttacker is Player player && Role == player.Role )
		{
			player.DropCarriable();
			player.TimeUntilClean = 20f;
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
		Game.AssertClient();

		if ( IsLocalPawn )
		{
			if ( Corpse.IsValid() )
				CameraMode.Current = new FollowEntityCamera( Corpse );
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

		info.Damage = Math.Min( Health, info.Damage );

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;
		LastDamage = info;

		Health -= info.Damage;

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
