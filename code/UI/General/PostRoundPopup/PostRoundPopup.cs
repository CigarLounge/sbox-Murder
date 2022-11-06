using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class PostRoundPopup : Panel
{
	private Panel Players { get; init; }
	private Label WinningText { get; init; }
	private Label Murderer { get; init; }

	public PostRoundPopup()
	{
		Players.AddChild( new Entry( "Matt", "Golf", 5 ) );
	}

	public void Close()
	{
		Delete();
	}
}
