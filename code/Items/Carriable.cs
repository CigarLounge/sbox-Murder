using Sandbox;

namespace Murder;

[Title( "Carriable" ), Icon( "luggage" )]
public abstract partial class Carriable : AnimatedEntity, IEntityHint, IUse
{
	[Net, Local, Predicted] public TimeSince TimeSinceDeployed { get; private set; }
	public TimeSince TimeSinceDropped { get; private set; }
	public virtual float DeployTime => 0;
	public Player PreviousOwner { get; private set; }
	public ModelEntity EffectEntity => (ViewModelEntity.IsValid() && IsFirstPersonMode) ? ViewModelEntity : this;
	public BaseViewModel ViewModelEntity { get; protected set; }
	public virtual string IconPath { get; }
	public virtual string ViewModelPath { get; }
	public virtual string WorldModelPath { get; }

	public new Player Owner
	{
		get => (Player)base.Owner;
		set => base.Owner = value;
	}

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
		TimeSinceDeployed = 0;

		if ( player == UI.Hud.DisplayedPlayer )
		{
			CreateViewModel();

			ViewModelEntity?.SetAnimParameter( "deploy", true );
		}
	}

	public virtual void ActiveEnd( Player player, bool dropped )
	{
		if ( !dropped )
			EnableDrawing = false;

		if ( Game.IsClient )
			DestroyViewModel();
	}

	public override void Simulate( IClient client ) { }

	public virtual void SimulateAnimator( CitizenAnimationHelper anim ) { }

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
		EnableDrawing = false;

		if ( !Game.IsServer )
			return;

		Owner = carrier;
		EnableAllCollisions = false;
	}

	public virtual void OnCarryDrop( Player dropper )
	{
		PreviousOwner = dropper;
		EnableDrawing = true;

		if ( !Game.IsServer )
			return;

		Owner = null;
		EnableAllCollisions = true;
		TimeSinceDropped = 0;
	}

	public override Sound PlaySound( string soundName, string attachment )
	{
		if ( Owner.IsValid() )
			return Owner.PlaySound( soundName, attachment );

		return base.PlaySound( soundName, attachment );
	}

	/// <summary>
	/// Create the viewmodel. You can override this in your base classes if you want
	/// to create a certain viewmodel entity.
	/// </summary>
	public virtual void CreateViewModel()
	{
		Game.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			EnableViewmodelRendering = true,
			Owner = Owner,
			Position = Position
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	/// <summary>
	/// We're done with the viewmodel - delete it
	/// </summary>
	public virtual void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsFirstPersonMode )
			DestroyViewModel();
	}

	bool IEntityHint.CanHint( Player player ) => Owner is null;

	bool IUse.OnUse( Entity user )
	{
		var player = (Player)user;

		if ( CanCarry( player ) )
			player.SetCarriable( this, true );

		return false;
	}

	bool IUse.IsUsable( Entity user ) => Owner is null && user is Player;
}
