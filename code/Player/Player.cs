using Sandbox;

namespace Murder;

[Title( "Player" ), Icon( "emoji_people" )]
public partial class Player : AnimatedEntity
{
	[Net]
	public string AssignedName { get; set; }

	[Net]
	public Color32 AssignedColour { get; set; }

	[Net]
	public int CluesCollected { get; set; }

	[Net]
	public string SteamName { get; private set; }

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

	public Corpse Corpse { get; set; }

	public Player( Client client ) : this()
	{
		client.Pawn = this;
		SteamName = client.Name;

		ClothingContainer.LoadFromClient( client );
		_avatarClothes = new( ClothingContainer.Clothing );
	}

	public Player() { }

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );
		Tags.Add( "solid" );

		SetModel( "models/citizen/citizen.vmdl" );

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

		DropCarriable()?.Delete();
		DeleteFlashlight();
		ResetDamageData();
		Client.SetValue( Strings.Spectator, IsForcedSpectator );
		Role = Role.None;

		Velocity = Vector3.Zero;
		WaterLevel = 0;

		if ( !IsForcedSpectator )
		{
			Health = MaxHealth;
			Client.VoiceStereo = true;
			LifeState = LifeState.Alive;

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableTouch = true;

			Controller = new WalkController();
			Camera = new FirstPersonCamera();

			CreateHull();
			CreateFlashlight();
			DressPlayer();
			ResetInterpolation();

			Event.Run( GameEvent.Player.Spawned, this );
			Game.Current.State.OnPlayerSpawned( this );
		}
		else
		{
			LifeState = LifeState.Dead;
			MakeSpectator( false );
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

		if ( IsSpectator )
			return;

		CreateFlashlight();

		Event.Run( GameEvent.Player.Spawned, this );
	}

	public override void Simulate( Client client )
	{
		var controller = GetActiveController();
		controller?.Simulate( client, this, Animator );

		if ( Input.Pressed( InputButton.Menu ) )
		{
			if ( ActiveCarriable.IsValid() && _lastKnownCarriable.IsValid() )
				(ActiveCarriable, _lastKnownCarriable) = (_lastKnownCarriable, ActiveCarriable);
		}
		else if ( Input.Pressed( InputButton.Slot1 ) )
			ActiveCarriable = null;
		else if ( Input.Pressed( InputButton.Slot2 ) )
			ActiveCarriable ??= Carriable;

		SimulateActiveCarriable();

		if ( this.IsAlive() )
		{
			SimulateFlashlight();
		}

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
		ActiveCarriable?.FrameSimulate( client );
	}

	/// <summary>
	/// Called after the camera setup logic has run. Allow the player to
	/// do stuff to the camera, or using the camera. Such as positioning entities
	/// relative to it, like viewmodels etc.
	/// </summary>
	public override void PostCameraSetup( ref CameraSetup setup )
	{
		ActiveCarriable?.PostCameraSetup( ref setup );
	}

	/// <summary>
	/// Called from the gamemode, clientside only.
	/// </summary>
	public override void BuildInput( InputBuilder input )
	{
		if ( input.StopProcessing )
			return;

		ActiveCarriable?.BuildInput( input );

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
		//if ( (Local.Pawn as Player).Role == Role.Murderer )
		var decal = ResourceLibrary.Get<DecalDefinition>( "decals/footprint.decal" );
		Decal.Place( decal, trace.Entity, trace.Bone, trace.EndPosition, Rotation.LookAt( trace.Normal + EyeRotation.Backward / 100 ), AssignedColour );
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

	public override void StartTouch( Entity other )
	{
		if ( !IsServer )
			return;

		if ( other is Carriable carriable && Carriable is null )
			SetCarriable( carriable );
	}

	[Net]
	public Carriable Carriable { get; private set; }

	public void SetCarriable( Carriable carriable, bool makeActive = false )
	{
		if ( Carriable is not null )
		{
			ActiveCarriable = null;
			Carriable.Parent = null;
			Carriable.OnCarryDrop( this );
		}

		if ( !carriable?.CanCarry( this ) ?? false )
			return;

		Carriable = carriable;

		if ( Carriable is not null )
		{
			Carriable.SetParent( this, true );
			Carriable.OnCarryStart( this );

			if ( makeActive )
				ActiveCarriable = Carriable;
		}
	}

	public Carriable DropCarriable()
	{
		if ( Carriable is null )
			return null;

		var dropped = Carriable;
		SetCarriable( null );

		if ( dropped.PhysicsGroup is not null )
			dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300f;

		return dropped;
	}

	#region ActiveCarriable
	[Net, Predicted]
	public Carriable ActiveCarriable { get; set; }

	public Carriable _lastActiveCarriable;
	public Carriable _lastKnownCarriable;

	public void SimulateActiveCarriable()
	{
		if ( _lastActiveCarriable != ActiveCarriable )
		{
			OnActiveCarriableChanged( _lastActiveCarriable, ActiveCarriable );
			_lastKnownCarriable = _lastActiveCarriable;
			_lastActiveCarriable = ActiveCarriable;
		}

		if ( !ActiveCarriable.IsValid() || !ActiveCarriable.IsAuthority )
			return;

		if ( ActiveCarriable.TimeSinceDeployed > ActiveCarriable.DeployTime )
			ActiveCarriable.Simulate( Client );
	}

	public void OnActiveCarriableChanged( Carriable previous, Carriable next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}
	#endregion

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
