using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class NameDisplay : Panel
{
	private Label Name { get; init; }

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		Name.Text = player.CurrentPlayer.AssignedName;
		Name.Style.FontColor = player.CurrentPlayer.AssignedColour;
	}
}
