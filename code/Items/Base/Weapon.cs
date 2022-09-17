using Murder;
using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

[Title( "Weapon" ), Icon( "sports_martial_arts" )]
public abstract partial class Weapon : Carriable
{
	[Net, Predicted]
	public int AmmoClip { get; protected set; }

	[Net, Predicted]
	public int ReserveAmmo { get; protected set; }

	[Net, Local, Predicted]
	public bool IsReloading { get; protected set; }

	[Net, Local, Predicted]
	public TimeSince TimeSincePrimaryAttack { get; protected set; }

	[Net, Local, Predicted]
	public TimeSince TimeSinceSecondaryAttack { get; protected set; }

	[Net, Local, Predicted]
	public TimeSince TimeSinceReload { get; protected set; }

	public virtual float Damage => 0;
	public virtual int ClipSize => 1;
	public virtual float PrimaryRate => 0;
	public virtual float ReloadTime => 0;
	public override string SlotText => $"{AmmoClip} + {ReserveAmmo}";
	// private Vector3 RecoilOnShoot => new( Rand.Float( -Info.HorizontalRecoilRange, Info.HorizontalRecoilRange ), Info.VerticalRecoil, 0 );
	private Vector3 CurrentRecoil { get; set; } = Vector3.Zero;

	public override void ActiveStart( Player player )
	{
		base.ActiveStart( player );

		IsReloading = false;
		TimeSinceReload = 0;
	}

	public override void Simulate( Client client )
	{
		if ( CanReload() )
		{
			Reload();
			return;
		}

		if ( !IsReloading )
		{
			if ( CanPrimaryAttack() )
			{
				using ( LagCompensation() )
				{
					TimeSincePrimaryAttack = 0;
					AttackPrimary();
				}
			}
		}
		else if ( TimeSinceReload > ReloadTime )
			OnReloadFinish();
	}

	/*
	public override void BuildInput( InputBuilder input )
	{
		
		base.BuildInput( input );

		var oldPitch = input.ViewAngles.pitch;
		var oldYaw = input.ViewAngles.yaw;

		input.ViewAngles.pitch -= CurrentRecoil.y * Time.Delta;
		input.ViewAngles.yaw -= CurrentRecoil.x * Time.Delta;

		CurrentRecoil -= CurrentRecoil
			.WithY( (oldPitch - input.ViewAngles.pitch) * Info.RecoilRecoveryScale )
			.WithX( (oldYaw - input.ViewAngles.yaw) * Info.RecoilRecoveryScale );
		
	}
*/
	protected virtual bool CanPrimaryAttack()
	{
		var rate = PrimaryRate;
		if ( rate <= 0 )
			return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	protected virtual void AttackPrimary()
	{
		if ( AmmoClip == 0 )
		{
			DryFireEffects();
			// PlaySound( Info.DryFireSound );

			return;
		}

		AmmoClip--;

		Owner.SetAnimParameter( "b_attack", true );
		ShootEffects();
		// PlaySound( Info.FireSound );

		ShootBullet( 0, 1.5f, Damage, 3.0f, 1 );
	}

	protected virtual bool CanReload()
	{
		if ( IsReloading )
			return false;

		if ( !Input.Pressed( InputButton.Reload ) )
			return false;

		if ( AmmoClip >= ClipSize || ReserveAmmo <= 0 )
			return false;

		return true;
	}

	protected virtual void Reload()
	{
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		Owner.SetAnimParameter( "b_reload", true );
		ReloadEffects();
	}

	protected virtual void OnReloadFinish()
	{
		IsReloading = false;
		AmmoClip += TakeAmmo( ClipSize - AmmoClip );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		if ( !Info.MuzzleFlashParticle.IsNullOrEmpty() )
			Particles.Create( Info.MuzzleFlashParticle, EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		CurrentRecoil += RecoilOnShoot;
	}

	[ClientRpc]
	protected virtual void DryFireEffects()
	{
		ViewModelEntity?.SetAnimParameter( "dryfire", true );
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}

	protected virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount )
	{
		// Seed rand using the tick, so bullet cones match on client and server
		Rand.SetSeed( Time.Tick );

		while ( bulletCount-- > 0 )
		{
			var forward = Owner.EyeRotation.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			foreach ( var trace in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 20000f, bulletSize ) )
			{
				trace.Surface.DoBulletImpact( trace );

				var fullEndPosition = trace.EndPosition + trace.Direction * bulletSize;

				if ( !IsServer )
					continue;

				if ( !trace.Entity.IsValid() )
					continue;

				using ( Prediction.Off() )
				{
					if ( Damage <= 0 )
						continue;

					var damageInfo = DamageInfo.FromBullet( trace.EndPosition, forward * 100f * force, damage )
						.UsingTraceResult( trace )
						.WithAttacker( Owner )
						.WithWeapon( this );

					trace.Entity.TakeDamage( damageInfo );
				}
			}
		}
	}

	/// <summary>
	/// Does a trace from start to end, does bullet impact effects. Coded as an IEnumerable so you can return multiple
	/// hits, like if you're going through layers or ricocet'ing or something.
	/// </summary>
	protected IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		var underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "glass", "interactable" )
				.Ignore( this )
				.Size( radius );

		//
		// If we're not underwater then we can hit water
		//
		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;

		//
		// Another trace, bullet going through thin material, penetrating water surface?
		//
	}

	protected int TakeAmmo( int ammo )
	{
		var available = Math.Min( ReserveAmmo, ammo );

		ReserveAmmo -= available;

		return available;
	}

	public static float GetDamageFalloff( float distance, float damage, float start, float end )
	{
		if ( end > 0f )
		{
			if ( start > 0f )
			{
				if ( distance < start )
					return damage;

				var falloffRange = end - start;
				var difference = (distance - start);

				return Math.Max( damage - (damage / falloffRange) * difference, 0f );
			}

			return Math.Max( damage - (damage / end) * distance, 0f );
		}

		return damage;
	}
}
