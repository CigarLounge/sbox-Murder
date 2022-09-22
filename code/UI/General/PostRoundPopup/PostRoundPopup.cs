using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Murder.UI;

[UseTemplate]
public class PostRoundPopup : Panel
{
	public static PostRoundPopup Instance { get; private set; }
	private Label Header { get; init; }
	private Label Content { get; init; }

	public PostRoundPopup() => Instance = this;

	[GameEvent.Round.End]
	private static void DisplayWinner( Role winningRole )
	{
		if ( !Host.IsClient )
			return;

		Local.Hud.AddChild( new PostRoundPopup() );

		Instance.Header.Text = winningRole == Role.None ? "IT'S A TIE!" : $"THE {winningRole.GetTitle()} WIN!";
		Instance.Header.Style.FontColor = winningRole.GetColor();

		Instance.Content.Text = $"The murderer was {Role.Murderer.GetPlayers().First().SteamName}";
	}

	[Event.Entity.PostCleanup]
	private void Close()
	{
		Delete();
		Instance = null;
	}
}
