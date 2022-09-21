using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class InventoryWrapper : Panel
{
	private Slot _holsterSlot;
	private Slot _carriableSlot;

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		var hasCarriableSlot = player.CurrentPlayer.IsAlive() && player.CurrentPlayer.Carriable.IsValid();
		if ( hasCarriableSlot )
			_carriableSlot ??= AddSlot( new Slot( player.CurrentPlayer.Carriable.Name, InputButton.Slot2 ) );
		else
			DeleteSlot( ref _carriableSlot );

		var hasHolsterSlot = player.CurrentPlayer.IsAlive();
		if ( hasHolsterSlot )
			_holsterSlot ??= AddSlot( new Slot( "Holster", InputButton.Slot1 ) );
		else
			DeleteSlot( ref _holsterSlot );
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
