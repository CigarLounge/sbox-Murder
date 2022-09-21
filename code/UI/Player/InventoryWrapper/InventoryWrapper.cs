using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class InventoryWrapper : Panel
{
	private readonly float _activeOpacity = 1f;
	private readonly float _inactiveOpacity = 0.4f;

	private Slot _holsterSlot;
	private Slot _carriableSlot;

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		var hasHolsterSlot = player.CurrentPlayer.IsAlive();
		if ( hasHolsterSlot )
			_holsterSlot ??= AddSlot( new Slot( "Holster", InputButton.Slot1 ) );
		else
			DeleteSlot( ref _holsterSlot );

		var hasCarriableSlot = hasHolsterSlot && player.CurrentPlayer.Carriable.IsValid();
		if ( hasCarriableSlot )
			_carriableSlot ??= AddSlot( new Slot( player.CurrentPlayer.Carriable.Name, InputButton.Slot2 ) );
		else
			DeleteSlot( ref _carriableSlot );

		var isCarriableSlotActive = player.CurrentPlayer.ActiveCarriable.IsValid();

		if ( _holsterSlot.IsValid() )
			_holsterSlot.Style.Opacity = isCarriableSlotActive ? _inactiveOpacity : _activeOpacity;

		if ( _carriableSlot.IsValid() )
			_carriableSlot.Style.Opacity = isCarriableSlotActive ? _activeOpacity : _inactiveOpacity;
	}

	private Slot AddSlot( Slot slot )
	{
		AddChild( slot );
		return slot;
	}

	private static void DeleteSlot( ref Slot slot )
	{
		slot?.Delete();
		slot = null;
	}
}
