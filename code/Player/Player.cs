using Sandbox;

namespace Murder;

[Title( "Player" ), Icon( "emoji_people" )]
public partial class Player : AnimatedEntity
{
	public bool IsForcedSpectator => Client.GetClientData<bool>( "forced_spectator" );
	public bool IsFrozen => (GameState.Current is GameplayState or MapSelectionState) && !GameState.Current.TimeLeft;

	public Player() { }

	public Player( IClient client ) : this()
	{
		client.Pawn = this;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "ignorereset", "player", "solid" );

		Health = 0;
		LifeState = LifeState.Respawnable;
		Transmit = TransmitType.Always;

		EnableAllCollisions = false;
		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableLagCompensation = true;
		EnableShadowInFirstPerson = true;
		EnableTouch = false;

		SetModel( "models/citizen/citizen.vmdl" );
	}

	public void Respawn()
	{
		Game.AssertServer();

		LifeState = LifeState.Respawnable;

		IsHolstered = true;
		Carriable?.Delete();
		Carriable = null;
		DeleteFlashlight();
		ResetDamageData();
		ResetInformation();

		Velocity = Vector3.Zero;

		if ( !IsForcedSpectator )
		{
			Health = 100f;
			LifeState = LifeState.Alive;
			TimeUntilClean = 0;
			Controller = new WalkController();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableTouch = true;

			CreateHull();
			CreateFlashlight();
			ResetInterpolation();
			DressPlayer();

			Event.Run( GameEvent.Player.Spawned, this );
			GameState.Current.OnPlayerSpawned( this );
		}
		else
		{
			LifeState = LifeState.Dead;
			MakeSpectator();
		}

		ClientRespawn( this );
	}

	public void MakeSpectator()
	{
		Client.Voice.WantsStereo = true;
		EnableAllCollisions = false;
		EnableDrawing = false;
		EnableTouch = false;
		Controller = null;
		Health = 0f;
		LifeState = LifeState.Dead;
	}

	internal bool temp;
	private void ClientRespawn()
	{
		Game.AssertClient();

		DeleteFlashlight();
		ResetDamageData();

		if ( !IsLocalPawn )
			Role = Role.None;
		else
		{
			MuteFilter = MuteFilter.None;
		}

		temp = true;
		if ( IsLocalPawn )
			CameraMode.Current = new FirstPersonCamera();

		if ( !this.IsAlive() )
			return;

		CreateFlashlight();

		Event.Run( GameEvent.Player.Spawned, this );
	}

	public override void Simulate( IClient client )
	{
		var controller = GetActiveController();
		Controller?.SetActivePlayer( this );
		controller?.Simulate();
		SimulateAnimation( Controller );

		if ( !this.IsAlive() )
			return;

		if ( Carriable.IsValid() )
			if ( Input.Pressed( InputButton.Menu ) || Input.Pressed( InputButton.Slot1 ) )
				IsHolstered = !IsHolstered;

		SimulateCarriable();
		SimulateFlashlight();

		if ( Game.IsServer )
			PlayerUse();

		if ( Role != Role.Murderer )
			return;

		if ( TimeUntilClean < -GameManager.MurdererFogTime )
			Components.GetOrCreate<MurdererFog>();
	}

	public override void FrameSimulate( IClient client )
	{
		Controller?.SetActivePlayer( this );
		Controller?.FrameSimulate();
		Carriable?.FrameSimulate( client );

		DisplayEntityHints();
	}

	#region Animator
	private void SimulateAnimation( WalkController controller )
	{
		if ( controller == null )
			return;

		// where should we be rotated to
		var turnSpeed = 0.02f;

		Rotation rotation;

		// If we're a bot, spin us around 180 degrees.
		if ( Client.IsBot )
			rotation = ViewAngles.WithYaw( ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		var animHelper = new CitizenAnimationHelper( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.IsClient && Client.IsValid()) ? Client.Voice.LastHeard < 0.5f ? Client.Voice.CurrentLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsNoclipping = controller.HasTag( "noclip" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = this.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;

		if ( controller.HasEvent( "jump" ) )
			animHelper.TriggerJump();

		if ( IsHolstered != _wasHolstered )
			animHelper.TriggerDeploy();

		if ( !IsHolstered )
			Carriable.SimulateAnimator( animHelper );
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}
	}

	public static DecalDefinition Footprint { get; internal set; }

	TimeSince _timeSinceLastFootstep;

	/// <summary>
	/// A foostep has arrived!
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !Game.IsClient || !this.IsAlive() )
			return;

		if ( _timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		if ( volume <= 1f )
			return;

		_timeSinceLastFootstep = 0;

		DoFootstep( pos, foot, volume );
	}

	[ClientRpc]
	private void DoFootstep( Vector3 pos, int foot, float volume )
	{
		var trace = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !trace.Hit )
			return;

		trace.Surface.DoFootstep( this, trace, foot, volume );

		if ( ((Player)Game.LocalPawn).Role != Role.Murderer )
			return;

		Decal.Place( Footprint, trace.Entity, trace.Bone, trace.EndPosition, Rotation.LookAt( trace.Normal, Rotation.Forward ), Color );
	}

	public float FootstepVolume()
	{
		if ( Controller.Duck.IsActive )
			return 0;

		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 3.0f;
	}
	#endregion

	#region Controller
	[Net, Predicted]
	public WalkController Controller { get; set; }

	[Net, Predicted]
	public WalkController DevController { get; set; }

	public WalkController GetActiveController()
	{
		return DevController ?? Controller;
	}
	#endregion

	public void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
		EnableHitboxes = true;
	}

	public override void Touch( Entity other )
	{
		if ( !Game.IsServer )
			return;

		if ( other is Carriable carriable && !Carriable.IsValid() )
			SetCarriable( carriable );
	}

	protected override void OnDestroy()
	{
		if ( Game.IsServer )
		{
			Corpse?.Delete();
			Corpse = null;
		}

		RoleExtensions._players[(int)Role].Remove( this );
		DeleteFlashlight();

		base.OnDestroy();
	}

	[ClientRpc]
	public static void ClientRespawn( Player player )
	{
		if ( !player.IsValid() )
			return;

		player.ClientRespawn();
	}
}
