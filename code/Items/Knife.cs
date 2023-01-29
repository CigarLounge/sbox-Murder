using Sandbox;
using Sandbox.Component;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_knife" )]
[EditorModel( "models/weapons/w_knife.vmdl" )]
[Title( "Knife" )]
public partial class Knife : Carriable
{
	[Net, Local, Predicted]
	public TimeSince TimeSinceStab { get; private set; }

	public override string IconPath { get; } = "ui/knife.png";
	public override string ViewModelPath { get; } = "models/weapons/v_knife.vmdl";
	public override string WorldModelPath { get; } = "models/weapons/w_knife.vmdl";

	private const string SwingSound = "knife_swing-1";
	private const string FleshHit = "knife_flesh_hit-1";

	private bool _isThrown = false;
	private Rotation _throwRotation = Rotation.From( new Angles( 90, 0, 0 ) );
	private float _gravityModifier;

	/*	public override void ClientSpawn()
		{
			var glow = Components.GetOrCreate<Glow>();
			glow.Color = Role.Murderer.GetColor();
			glow.ObscuredColor = Color.Transparent;
		}*/

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

	/*	public override void OnCarryStart( Player carrier )
		{
			if ( Game.LocalPawn is Player local && local.Role == Role.Murderer )
				Components.GetOrCreate<Glow>().Enabled = false;

			base.OnCarryStart( carrier );
		}

		public override void OnCarryDrop( Player dropper )
		{
			if ( Game.LocalPawn is Player local && local.Role == Role.Murderer )
				Components.GetOrCreate<Glow>().Enabled = true;

			base.OnCarryDrop( dropper );
		}*/

	private void MeleeAttack( float damage, float range, float radius )
	{
		TimeSinceStab = 0;

		Owner.SetAnimParameter( "b_attack", true );
		SwingEffects();
		PlaySound( SwingSound );

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
			PlaySound( FleshHit );

		trace.Entity.TakeDamage( damageInfo );
	}

	private void Throw()
	{
		var trace = Trace.Ray( Owner.EyePosition, Owner.EyePosition )
			.Ignore( Owner )
			.Run();

		_isThrown = true;
		_gravityModifier = 0;

		if ( !Game.IsServer )
			return;

		Owner.DropCarriable();

		PhysicsEnabled = true;
		Position = trace.EndPosition;
		Rotation = PreviousOwner.EyeRotation * _throwRotation;
		
		Velocity =  PreviousOwner.EyeRotation.Forward * (700f + PreviousOwner.Velocity.Length) + Vector3.Up * 200 ;
		ApplyLocalAngularImpulse( new Vector3( 0, 1500 , 0 ) );
	}

	public override void StartTouch( Entity other )
	{
		if ( other is Player player && player != PreviousOwner )
		{
			_isThrown = false;

			var damageInfo = DamageInfo.Generic( 200f )
				.WithPosition( Position )
				.WithForce( Position.Normal * 20f )
				.WithAttacker( PreviousOwner )
				.WithWeapon( this )
				.WithTag( "slash" );

			player.TakeDamage( damageInfo );
		}
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		if ( !_isThrown || Velocity.Length < 500f )
			return;

		_isThrown = false;

		if ( !eventData.Other.Entity.IsWorld )
			return;

		var dot = Vector3.Dot( eventData.Normal, (Rotation *_throwRotation).Backward );

		if ( dot < 0.707f )
			return;

		eventData.Other.Surface.DoBulletImpact( Trace.Ray( Position, eventData.Position ).Run() );
		Position = eventData.Position;
		PhysicsEnabled = false;
	}

	[ClientRpc]
	protected void SwingEffects()
	{
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}
}
