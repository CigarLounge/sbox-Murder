using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class ScoreboardGroup : Panel
{
	public int GroupMembers = 0;

	private Label Title { get; init; }
	private Panel Content { get; init; }

	public ScoreboardGroup( Panel parent, bool spectators ) : base( parent )
	{
		Title.Text = spectators ? "Spectators" : "Players";
		AddClass( spectators ? "spectators" : "players" );
	}

	public ScoreboardEntry AddEntry( Client client )
	{
		var scoreboardEntry = new ScoreboardEntry( Content, client );
		scoreboardEntry.Update();
		return scoreboardEntry;
	}
}
