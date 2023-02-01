using Sandbox;
using Sandbox.Component;
using System;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_knife" )]
[EditorModel( "models/weapons/w_knife.vmdl" )]
[Title( "Knife" )]
public partial class Knife : Carriable
{
	[Net, Local, Predicted] public TimeSince TimeSinceStab { get; private set; }
	public override string IconPath { get; } = "ui/knife.png";
	public override string ViewModelPath { get; } = "models/weapons/v_knife.vmdl";
	public override string WorldModelPath { get; } = "models/weapons/w_knife.vmdl";

	private bool _isThrown = false;
	private Particles _blackSmoke;

	public override void Simulate( IClient client )
	{
		if ( TimeSinceStab < 1.5f )
			return;

		if ( Input.Down( InputButton.PrimaryAttack ) )
		{
			using ( LagCompensation() )
			{
				MeleeAttack( 120f, 100f, 8f );
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
		return !_isThrown && carrier.Role == Role.Murderer && base.CanCarry( carrier );
	}

	public override void OnCarryStart( Player carrier )
	{
		if ( Game.LocalPawn is Player player && player.Role == Role.Murderer )
			Components.GetOrCreate<Glow>().Enabled = false;

		_blackSmoke?.Destroy();

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

	private void MeleeAttack( float damage, float range, float radius )
	{
		TimeSinceStab = 0;

		Owner.SetAnimParameter( "b_attack", true );
		SwingEffects();
		PlaySound( "knife_swing-1" );

		var endPosition = Owner.EyePosition + Owner.EyeRotation.Forward * range;

		var trace = Trace.Ray( Owner.EyePosition, endPosition )
			.UseHitboxes( true )
			.Ignore( Owner )
			.Radius( radius )
			.Run();

		if ( !trace.Hit )
			return;

		trace.Surface.DoBulletImpact( trace );

		if ( !Game.IsServer )
			return;

		var damageInfo = DamageInfo.Generic( damage )
			.WithPosition( trace.EndPosition )
			.UsingTraceResult( trace )
			.WithForce( trace.Direction * 200f )
			.WithAttacker( Owner )
			.WithWeapon( this )
			.WithTag( "slash" );

		if ( trace.Entity is Player )
			PlaySound( "knife_flesh_hit-1" );

		trace.Entity.TakeDamage( damageInfo );
	}

	private void Throw()
	{
		var trace = Trace.Ray( Owner.EyePosition, Owner.EyePosition )
			.Ignore( Owner )
			.Run();

		_isThrown = true;

		if ( !Game.IsServer )
			return;

		using ( Prediction.Off() )
		{
			Owner.DropCarriable();

			PhysicsEnabled = true;
			Position = trace.EndPosition;
			Rotation = PreviousOwner.EyeRotation;

			_blackSmoke = Particles.Create( "particles/black_smoke.vpcf", this );

			Velocity = PreviousOwner.EyeRotation.Forward * 700f + Vector3.Up * 200;
			ApplyLocalAngularImpulse( new Vector3( 0, 1500, 0 ) );
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( !_isThrown || other is not Player player || player == PreviousOwner )	
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

		eventData.Other.Surface.DoBulletImpact( Trace.Ray( Position, eventData.Position ).Ignore( this ).Run() );
		Position = eventData.Position;
		PhysicsEnabled = false;
	}

	[ClientRpc]
	protected void SwingEffects()
	{
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}
}
