using Sandbox;

namespace Murder;

[Title( "Carriable" ), Icon( "luggage" )]
public abstract partial class Carriable : AnimatedEntity, IEntityHint, IUse
{
	[Net, Local, Predicted]
	public TimeSince TimeSinceDeployed { get; private set; }

	public TimeSince TimeSinceDropped { get; private set; }

	public new Player Owner
	{
		get => (Player)base.Owner;
		set => base.Owner = value;
	}

	public virtual float DeployTime => 0;
	public BaseViewModel HandsModelEntity { get; private set; }
	public Player PreviousOwner { get; private set; }
	public BaseViewModel ViewModelEntity { get; protected set; }
	public virtual string IconPath { get; }
	public virtual string ViewModelPath { get; }
	public virtual string WorldModelPath { get; }

	/// <summary>
	/// Return the entity we should be spawning particles from.
	/// </summary>
	public ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;

	public bool IsActive => !Owner?.IsHolstered ?? false;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "interactable" );
		PhysicsEnabled = true;
		UsePhysicsCollision = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetModel( WorldModelPath );
	}

	public virtual void ActiveStart( Player player )
	{
		EnableDrawing = true;

		var animator = player.Animator;

		if ( animator is not null )
			SimulateAnimator( animator );

		if ( IsLocalPawn )
		{
			CreateViewModel();
			CreateHudElements();

			ViewModelEntity?.SetAnimParameter( "deploy", true );
		}

		TimeSinceDeployed = 0;
	}

	public virtual void ActiveEnd( Player player, bool dropped )
	{
		if ( !dropped )
			EnableDrawing = false;

		if ( IsClient )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	public override void Simulate( Client client ) { }

	public virtual bool CanCarry( Player carrier )
	{
		if ( Owner is not null )
			return false;

		if ( carrier == PreviousOwner && TimeSinceDropped <= 1f )
			return false;

		return true;
	}

	public virtual void OnCarryStart( Player carrier )
	{
		if ( !IsServer )
			return;

		Owner = carrier;
		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public virtual void OnCarryDrop( Player dropper )
	{
		PreviousOwner = dropper;

		if ( !IsServer )
			return;

		Owner = null;
		EnableDrawing = true;
		EnableAllCollisions = true;
		TimeSinceDropped = 0;
	}

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	public virtual void SimulateAnimator( PawnAnimator animator )
	{
		animator.SetAnimParameter( "aim_body_weight", 1.0f );
		animator.SetAnimParameter( "holdtype_handedness", 0 );
	}

	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
	protected virtual void CreateViewModel()
	{
		Host.AssertClient();

		if ( ViewModelPath.IsNullOrEmpty() )
			return;

		ViewModelEntity = new ViewModel
		{
			EnableViewmodelRendering = true,
			Owner = Owner,
			Position = Position
		};

		ViewModelEntity.SetModel( ViewModelPath );

		HandsModelEntity = new BaseViewModel
		{
			EnableViewmodelRendering = true,
			Owner = Owner,
			Position = Position
		};

		HandsModelEntity.SetModel( "models/weapons/v_arms_ter.vmdl" );
		HandsModelEntity.SetParent( ViewModelEntity, true );
	}

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	protected virtual void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
		HandsModelEntity?.Delete();
		HandsModelEntity = null;
	}

	protected virtual void CreateHudElements() { }

	protected virtual void DestroyHudElements() { }

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsFirstPersonMode )
		{
			DestroyViewModel();
			DestroyHudElements();
		}
	}

	bool IEntityHint.CanHint( Player player ) => Owner is null;

	bool IUse.OnUse( Entity user )
	{
		var player = (Player)user;

		if ( CanCarry( player ) )
			player.SetCarriable( this );

		return false;
	}

	bool IUse.IsUsable( Entity user ) => Owner is null && user is Player;
}
