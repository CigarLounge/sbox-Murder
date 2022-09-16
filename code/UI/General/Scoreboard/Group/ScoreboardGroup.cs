using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class ScoreboardGroup : Panel
{
	public readonly LifeState GroupStatus;
	public int GroupMembers = 0;

	private Label Title { get; init; }
	private Panel Content { get; init; }

	public ScoreboardGroup( Panel parent, LifeState GroupStatus ) : base( parent )
	{
		this.GroupStatus = GroupStatus;
		Title.Text = GroupStatus.ToString();
		AddClass( GroupStatus.ToString() );
	}

	public ScoreboardEntry AddEntry( Client client )
	{
		var scoreboardEntry = new ScoreboardEntry( Content, client )
		{
			PlayerStatus = GroupStatus
		};

		scoreboardEntry.Update();

		return scoreboardEntry;
	}
}
