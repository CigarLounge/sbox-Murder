using Sandbox;
using Sandbox.Component;
using System;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "mur_weapon_knife" )]
[EditorModel( "models/knife/wm_knife.vmdl" )]
[Title( "Knife" )]
public partial class Knife : Carriable
{
	[Net, Local, Predicted] public TimeSince TimeSinceSwing { get; private set; }
	public override float DeployTime => 0.6f;
	public override string IconPath { get; } = "/ui/knife.png";
	public override string ViewModelPath { get; } = "models/knife/vm_knife.vmdl";
	public override string WorldModelPath { get; } = "models/knife/wm_knife.vmdl";

	private Particles _fog;
	private bool _isThrown;

	public override void Simulate( IClient client )
	{
		if ( TimeSinceSwing < 1f )
			return;

		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			using ( LagCompensation() )
			{
				Swing();
			}
		}
		else if ( Input.Released( InputButton.SecondaryAttack ) )
		{
			using ( LagCompensation() )
			{
				Throw();
			}
		}
	}

	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		anim.HoldType = CitizenAnimationHelper.HoldTypes.Punch;
		anim.AimBodyWeight = 1.0f;
		anim.Handedness = 0;
	}

	public override bool CanCarry( Player carrier )
	{
		if ( carrier.Role != Role.Murderer )
			return false;

		if ( _isThrown )
			return false;

		return base.CanCarry( carrier );
	}

	public override void OnCarryStart( Player carrier )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Murderer )
			Components.GetOrCreate<Glow>().Enabled = false;

		_fog?.Destroy();

		base.OnCarryStart( carrier );
	}

	public override void OnCarryDrop( Player dropper )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Murderer )
		{
			var glow = Components.GetOrCreate<Glow>();
			glow.Enabled = true;
			glow.Color = Role.Murderer.GetColor();
			glow.ObscuredColor = Color.Transparent;
		}

		base.OnCarryDrop( dropper );
	}

	private void Swing()
	{
		TimeSinceSwing = 0;
		Owner.SetAnimParameter( "b_attack", true );

		PlaySound( "swing" );
		SwingEffects();

		var trace = Trace.Ray( Owner.AimRay, 60f )
			.UseHitboxes( true )
			.Ignore( Owner )
			.Radius( 8f )
			.Run();

		if ( !trace.Hit )
			return;

		trace.Surface.DoBulletImpact( trace );

		if ( !Game.IsServer )
			return;

		var damageInfo = DamageInfo.Generic( 200f )
			.WithPosition( trace.EndPosition )
			.UsingTraceResult( trace )
			.WithForce( trace.Direction * 200f )
			.WithAttacker( Owner )
			.WithWeapon( this )
			.WithTag( "slash" );

		if ( trace.Entity is Player )
			PlaySound( "flesh_hit" );

		trace.Entity.TakeDamage( damageInfo );
	}

	private void Throw()
	{
		var tr = Trace.Ray( Owner.AimRay, 1f )
			.Ignore( Owner )
			.Run();

		_isThrown = true;

		if ( !Game.IsServer )
			return;

		using ( Prediction.Off() )
		{
			Owner.DropCarriable();

			PhysicsEnabled = true;
			Position = tr.EndPosition;
			Rotation = PreviousOwner.EyeRotation;

			_fog = Particles.Create( "particles/black_smoke.vpcf", this );

			Velocity = PreviousOwner.EyeRotation.Forward * 700f + Vector3.Up * 200;
			ApplyLocalAngularImpulse( new Vector3( 0, 1500, 0 ) );
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( !Game.IsServer || !_isThrown || other is not Player player || player == PreviousOwner )
			return;

		var damageInfo = DamageInfo.Generic( 200f )
			.WithPosition( Position )
			.WithForce( Position.Normal * 20f )
			.WithAttacker( PreviousOwner )
			.WithWeapon( this )
			.WithTag( "slash" );

		player.TakeDamage( damageInfo );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		_isThrown = false;

		if ( eventData.Speed < 500f )
			return;

		if ( !eventData.Other.Entity.IsWorld )
			return;

		var dot = Vector3.Dot( eventData.Normal, Rotation.Forward );

		if ( dot < MathF.Cos( MathF.PI / 4f ) )
			return;

		Position = eventData.Position;
		PhysicsEnabled = false;

		var tr = Trace.Ray( Position, eventData.Position )
			.Ignore( this )
			.Run();

		eventData.Other.Surface.DoBulletImpact( tr );

		Unstuck();
	}

	private async void Unstuck()
	{
		await GameTask.Delay( 10000 );

		if ( !Parent.IsValid() )
			PhysicsEnabled = true;
	}

	[ClientRpc]
	protected void SwingEffects()
	{
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}
}
