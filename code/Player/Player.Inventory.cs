using Sandbox;

namespace Murder;

public partial class Player
{
	[Net, Change]
	public Carriable Carriable { get; private set; }

	[Net, Predicted]
	public bool IsHolstered { get; private set; } = true;

	public void SetCarriable( Carriable carriable, bool makeActive = false )
	{
		Assert.NotNull( carriable );

		if ( !carriable.CanCarry( this ) )
			return;

		DropCarriable();

		carriable.SetParent( this, true );
		carriable.OnCarryStart( this );
		Carriable = carriable;

		IsHolstered = !makeActive;
	}

	public Carriable DropCarriable()
	{
		if ( Carriable is null )
			return null;

		if ( !IsHolstered )
			Carriable.ActiveEnd( this, true );

		Carriable.Parent = null;
		Carriable.OnCarryDrop( this );

		var dropped = Carriable;
		Carriable = null;
		IsHolstered = true;

		return dropped;
	}

	private bool _wasHolstered = true;

	public void SimulateCarriable()
	{
		if ( !Carriable.IsValid() || !Carriable.IsAuthority )
			return;

		if ( _wasHolstered != IsHolstered )
		{
			if ( _wasHolstered )
				Carriable.ActiveStart( this );
			else
				Carriable.ActiveEnd( this, false );

			_wasHolstered = IsHolstered;
		}

		if ( !IsHolstered && Carriable.TimeSinceDeployed > Carriable.DeployTime )
			Carriable.Simulate( Client );
	}

	private void OnCarriableChanged( Carriable oldVal, Carriable newVal )
	{
		if ( oldVal.IsValid() )
		{
			if ( !IsHolstered )
				oldVal.ActiveEnd( this, true );

			oldVal.OnCarryDrop( this );
		}

		if ( newVal.IsValid() )
			newVal.OnCarryStart( this );
	}
}