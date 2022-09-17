using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class RoleDisplay : Panel
{
	private Label Role { get; init; }

	public override void Tick()
	{
		var player = Local.Pawn as Player;

		this.Enabled( player.CurrentPlayer.IsValid() && player.CurrentPlayer.IsAlive() );
		if ( !this.IsEnabled() )
			return;

		Role.Text = player.CurrentPlayer.Role.GetTitle();
		Role.Style.FontColor = player.CurrentPlayer.Role.GetColor();
	}
}
