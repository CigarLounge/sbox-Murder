using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class NameHealthDisplay : Panel
{
	private Label Name { get; init; }
	private RadialHealth RadialHealth { get; init; }

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		Name.Text = player.CurrentPlayer.AssignedName;
		Name.Style.FontColor = player.CurrentPlayer.AssignedColor;

		RadialHealth.Radial.Style.Height = Length.Percent( player.CurrentPlayer.Health / Player.MaxHealth * 200 );
		RadialHealth.Radial.Style.BackgroundColor = player.CurrentPlayer.AssignedColor;

		RadialHealth.CluesCollected.Text = $"{player.CurrentPlayer.CluesCollected}";
	}
}
