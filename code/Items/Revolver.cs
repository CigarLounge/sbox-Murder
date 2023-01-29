using Sandbox;
using Sandbox.Component;
using System.Collections.Generic;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_revolver" )]
[EditorModel( "models/weapons/w_mr96.vmdl" )]
[Title( "Revolver" )]
public partial class Revolver : Carriable
{
	[Net, Predicted]
	public bool BulletInClip { get; private set; } = true;

	[Net, Local, Predicted]
	public TimeSince TimeSincePrimaryAttack { get; private set; }

	[Net, Local, Predicted]
	public bool IsReloading { get; private set; }

	[Net, Local, Predicted]
	public TimeSince TimeSinceReload { get; private set; }

	public override float DeployTime => 1.2f;
	public override string IconPath { get; } = "ui/revolver.png";
	public override string ViewModelPath { get; } = "models/weapons/v_mr96.vmdl";
	public override string WorldModelPath { get; } = "models/weapons/w_mr96.vmdl";

	/*public override void ClientSpawn()
	{
		var glow = Components.GetOrCreate<Glow>();
		glow.Color = Role.Bystander.GetColor();
		glow.ObscuredColor = Color.Transparent;
	}*/

	public override void ActiveStart( Player player )
	{
		base.ActiveStart( player );

		IsReloading = false;
	}

	public override void Simulate( IClient client )
	{
		if ( TimeSincePrimaryAttack <= 0.5f )
			return;

		if ( !BulletInClip && !IsReloading )
		{
			Reload();
			return;
		}

		if ( !IsReloading )
		{
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{
				using ( LagCompensation() )
				{
					AttackPrimary();
				}
			}
		}
		else if ( TimeSinceReload > 2.4f )
		{
			IsReloading = false;
			BulletInClip = true;
		}
	}

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		anim.AimBodyWeight = 1.0f;
		anim.Handedness = 0;
	}

	private void AttackPrimary()
	{
		BulletInClip = false;
		TimeSincePrimaryAttack = 0;

		Owner.SetAnimParameter( "b_attack", true );
		ShootEffects();
		PlaySound( "sounds/weapons/mr96/mr96_fire-1.sound" );

		ShootBullet();
	}

	private void Reload()
	{
		TimeSinceReload = 0;
		IsReloading = true;

		Owner.SetAnimParameter( "b_reload", true );
		ReloadEffects();
	}

	public override bool CanCarry( Player carrier )
	{
		return carrier.Role == Role.Bystander && carrier.TimeUntilClean && base.CanCarry( carrier );
	}

	/*public override void OnCarryStart( Player carrier )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Bystander )
			Components.GetOrCreate<Glow>().Enabled = false;

		base.OnCarryStart( carrier );
	}

	public override void OnCarryDrop( Player dropper )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Bystander )
			Components.GetOrCreate<Glow>().Enabled = true;

		base.OnCarryDrop( dropper );
	}*/

	protected void ShootBullet()
	{
		// Seed rand using the tick, so bullet cones match on client and server
		Game.SetRandomSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;

		foreach ( var trace in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 20000f, 3.0f ) )
		{
			trace.Surface.DoBulletImpact( trace );

			var fullEndPosition = trace.EndPosition + trace.Direction * 3.0f;

			if ( !Game.IsServer )
				continue;

			if ( !trace.Entity.IsValid() )
				continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( trace.EndPosition, forward * 250f, 200f )
					.UsingTraceResult( trace )
					.WithAttacker( Owner )
					.WithWeapon( this );

				trace.Entity.TakeDamage( damageInfo );
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

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Particles.Create( "particles/muzzle/flash_small.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
		// CurrentRecoil += RecoilOnShoot;
	}

	[ClientRpc]
	protected virtual void ReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}
}
