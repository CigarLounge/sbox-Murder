using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class RoleDisplay : Panel
{
	private Label Role { get; init; }

	public override void Tick()
	{
		var player = ((Player)Local.Pawn).CurrentPlayer;

		this.Enabled( player.IsValid() && player.IsAlive() );
		if ( !this.IsEnabled() )
			return;

		Role.Text = player.Role.GetTitle();
		Role.Style.FontColor = player.Role.GetColor();
	}
}
