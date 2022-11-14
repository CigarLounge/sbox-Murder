using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
internal class InventoryDisplay : Panel
{
	private Label Carriable { get; init; }
	private Panel Holster { get; init; }

	public override void Tick()
	{
		var player = (Player)Local.Pawn;

		this.Enabled( player.IsAlive() );
		if ( !this.IsEnabled() )
			return;

		Holster.SetClass( "active", player.IsHolstered );

		Carriable.Parent.Enabled( player.Carriable.IsValid() );
		if ( Carriable.Parent.IsEnabled() )
		{
			Carriable.Parent.SetClass( "active", !player.IsHolstered );
			Carriable.Text = player.Carriable.Name;
		}
	}
}
