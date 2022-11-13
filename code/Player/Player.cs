using Sandbox;

namespace Murder;

[Title( "Player" ), Icon( "emoji_people" )]
public partial class Player : AnimatedEntity
{
	public CameraMode Camera
	{
		get => Components.Get<CameraMode>();
		set
		{
			var current = Camera;
			if ( current == value )
				return;

			Components.RemoveAny<CameraMode>();
			Components.Add( value );
		}
	}

	public Player() { }

	public Player( Client client )
	{
		client.Pawn = this;
		SteamName = client.Name;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );
		Tags.Add( "solid" );

		SetModel( "models/citizen/citizen.vmdl" );
		DressPlayer();

		Health = 0;
		LifeState = LifeState.Respawnable;
		Transmit = TransmitType.Always;

		EnableAllCollisions = false;
		EnableDrawing = false;
		EnableHideInFirstPerson = true;
		EnableLagCompensation = true;
		EnableShadowInFirstPerson = true;
		EnableTouch = false;

		Animator = new PlayerAnimator();
		Camera = new FreeSpectateCamera();
	}

	public void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Respawnable;

		IsHolstered = true;
		Carriable?.Delete();
		Carriable = null;	
		DeleteFlashlight();
		ResetDamageData();
		ResetInformation();
		Client.SetValue( Strings.Spectator, IsForcedSpectator );

		Velocity = Vector3.Zero;
		WaterLevel = 0;

		if ( !IsForcedSpectator )
		{
			Client.VoiceStereo = true;
			Health = MaxHealth;
			LifeState = LifeState.Alive;
			TimeUntilClean = 0;

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableTouch = true;

			Controller = new WalkController();
			Camera = new FirstPersonCamera();

			CreateHull();
			CreateFlashlight();
			ResetInterpolation();

			Event.Run( GameEvent.Player.Spawned, this );
			Game.Current.State.OnPlayerSpawned( this );
		}
		else
		{
			LifeState = LifeState.Dead;
			MakeSpectator();
		}

		ClientRespawn( this );
	}

	private void ClientRespawn()
	{
		Host.AssertClient();

		DeleteFlashlight();
		ResetDamageData();

		if ( !IsLocalPawn )
			Role = Role.None;
		else
		{
			CurrentChannel = Channel.All;
			MuteFilter = MuteFilter.None;
		}

		if ( IsForcedSpectator )
			return;

		CreateFlashlight();

		Event.Run( GameEvent.Player.Spawned, this );
	}

	public override void Simulate( Client client )
	{
		var controller = GetActiveController();
		controller?.Simulate( client, this, Animator );

		if ( Carriable.IsValid() )
		{
			if ( Input.Pressed( InputButton.Menu ) && Carriable.IsValid() )
				IsHolstered = !IsHolstered;
			else if ( Input.Pressed( InputButton.Slot1 ) )
				IsHolstered = true;
			else if ( Input.Pressed( InputButton.Slot2 ) )
				IsHolstered = false;
		}

		SimulateCarriable();

		if ( this.IsAlive() )
			SimulateFlashlight();

		if ( IsClient )
		{
			if ( !this.IsAlive() )
				ChangeSpectateCamera();
		}
		else
		{
			CheckAFK();
			PlayerUse();
		}
	}

	public override void FrameSimulate( Client client )
	{
		var controller = GetActiveController();
		controller?.FrameSimulate( client, this, Animator );

		if ( WaterLevel > 0.9f )
		{
			Audio.SetEffect( "underwater", 1, velocity: 5.0f );
		}
		else
		{
			Audio.SetEffect( "underwater", 0, velocity: 1.0f );
		}

		DisplayEntityHints();
	}

	/// <summary>
	/// Called from the gamemode, clientside only.
	/// </summary>
	public override void BuildInput( InputBuilder input )
	{
		if ( input.StopProcessing )
			return;

		GetActiveController()?.BuildInput( input );

		if ( input.StopProcessing )
			return;

		Animator.BuildInput( input );
	}

	#region Animator
	[Net, Predicted]
	public PawnAnimator Animator { get; private set; }

	public static DecalDefinition Footprint;

	TimeSince _timeSinceLastFootstep;

	/// <summary>
	/// A foostep has arrived!
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( !this.IsAlive() )
			return;

		if ( !IsClient )
			return;

		if ( _timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		_timeSinceLastFootstep = 0;

		var trace = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !trace.Hit )
			return;

		trace.Surface.DoFootstep( this, trace, foot, volume );

		if ( ((Player)Local.Pawn).Role != Role.Murderer )
			return;

		if ( volume < 5 )
			return;

		Decal.Place( Footprint, trace.Entity, trace.Bone, trace.EndPosition, Rotation.LookAt( trace.Normal, Rotation.Forward ), Color );
	}

	public float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 5.0f;
	}
	#endregion

	#region Controller
	[Net, Predicted]
	public PawnController Controller { get; set; }

	[Net, Predicted]
	public PawnController DevController { get; set; }

	public PawnController GetActiveController()
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
		if ( !IsServer )
			return;

		if ( other is Carriable carriable && !Carriable.IsValid() )
			SetCarriable( carriable );
	}

	protected override void OnDestroy()
	{
		if ( IsServer )
		{
			Corpse?.Delete();
			Corpse = null;
		}

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
