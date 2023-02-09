using Sandbox;
using Sandbox.Diagnostics;

namespace Murder;

public partial class Player
{
	[Net, Predicted] internal Carriable ActiveCarriable { get; set; }
	public Carriable Carriable { get; private set; }
	public bool IsHolstered => ActiveCarriable != Carriable;

	public void SetCarriable( Carriable carriable, bool makeActive = false )
	{
		Assert.NotNull( carriable );

		if ( !carriable.CanCarry( this ) )
			return;

		DropCarriable();

		carriable.SetParent( this, true );

		if ( makeActive )
			ActiveCarriable = carriable;
	}

	public Carriable DropCarriable()
	{
		if ( Carriable is null )
			return null;

		var dropped = Carriable;

		dropped.Parent = null;

		return dropped;
	}

	private Carriable _lastActiveCarriable;

	public void SimulateActiveCarriable()
	{
		if ( _lastActiveCarriable != ActiveCarriable )
		{
			OnActiveCarriableChanged( _lastActiveCarriable, ActiveCarriable );
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
}
