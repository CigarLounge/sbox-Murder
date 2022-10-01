using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Murder.UI;
/*
[UseTemplate]
public class PostRoundPopup : Panel
{
	private static PostRoundPopup _instance;
	private Label Header { get; init; }
	private Label Content { get; init; }

	public PostRoundPopup() => _instance = this;

	[GameEvent.Round.End]
	private static void DisplayWinner( Role winningRole )
	{
		if ( !Host.IsClient )
			return;

		Local.Hud.AddChild( new PostRoundPopup() );

		_instance.Header.Text = winningRole == Role.None ? "IT'S A TIE!" : $"THE {winningRole.GetTitle()} WIN!";
		_instance.Header.Style.FontColor = winningRole.GetColor();

		_instance.Content.Text = $"The murderer was {Role.Murderer.GetPlayers().First().SteamName}";
	}

	[Event.Entity.PostCleanup]
	private void Close()
	{
		Delete();
		_instance = null;
	}
}
*/
