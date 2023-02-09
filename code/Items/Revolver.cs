using Sandbox;
using Sandbox.Component;
using System.Collections.Generic;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "mur_weapon_revolver" )]
[EditorModel( "models/revolver/wm_revolver.vmdl" )]
[Title( "Revolver" )]
public partial class Revolver : Carriable
{
	[Net, Local, Predicted] public bool BulletInClip { get; private set; } = true;
	[Net, Local, Predicted] public TimeSince TimeSincePrimaryAttack { get; private set; }
	[Net, Local, Predicted] public bool IsReloading { get; private set; }
	[Net, Local, Predicted] public TimeSince TimeSinceReload { get; private set; }
	public override float DeployTime => 0.3f;
	public override string IconPath { get; } = "/ui/revolver.png";
	public override string ViewModelPath { get; } = "models/revolver/vm_revolver.vmdl";
	public override string WorldModelPath { get; } = "models/revolver/wm_revolver.vmdl";

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
		else if ( TimeSinceReload > 2.1f )
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

		PlaySound( "fire" );
		ShootEffects();
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
		if ( carrier.Role != Role.Bystander )
			return false;

		if ( !carrier.TimeUntilClean )
			return false;

		return base.CanCarry( carrier );
	}

	public override void OnCarryStart( Player carrier )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Bystander )
			Components.GetOrCreate<Glow>().Enabled = false;

		base.OnCarryStart( carrier );
	}

	public override void OnCarryDrop( Player dropper )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Bystander )
		{
			var glow = Components.GetOrCreate<Glow>();
			glow.Enabled = true;
			glow.Color = Role.Bystander.GetColor();
			glow.ObscuredColor = Color.Transparent;
		}

		base.OnCarryDrop( dropper );
	}

	protected void ShootBullet()
	{
		// Seed rand using the tick, so bullet cones match on client and server
		Game.SetRandomSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;

		foreach ( var trace in TraceBullet( Owner.AimRay ) )
		{
			trace.Surface.DoBulletImpact( trace );

			if ( !Game.IsServer )
				continue;

			if ( !trace.Entity.IsValid() )
				continue;

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( trace.EndPosition, forward * 500f, 200f )
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
	protected IEnumerable<TraceResult> TraceBullet( Ray ray )
	{
		var underWater = Trace.TestPoint( ray.Position, "water" );

		var trace = Trace.Ray( ray, 20000f )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "glass", "interactable" )
				.Ignore( this )
				.Size( 3.0f );

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
	protected void ShootEffects()
	{
		Particles.Create( "particles/muzzle/flash_large.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	[ClientRpc]
	protected void ReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
	}
}
