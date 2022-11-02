using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class InventorySlot : Panel
{
	public Carriable Carriable { get; init; }
	public Label SlotLabel { get; set; }
	private Label SlotTitle { get; set; }

	public InventorySlot( Panel parent, Carriable carriable )
	{
		Parent = parent;
		Carriable = carriable;

		SlotLabel.Text = ((int)carriable.Slot + 1).ToString();
		SlotTitle.Text = carriable.Title;
	}

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		SlotLabel.Style.BackgroundColor = player.CurrentPlayer.Role.GetColor();
	}
}
