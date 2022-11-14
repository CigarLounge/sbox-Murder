using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Entry : Panel
{
	private Label Name { get; init; }
	private Label AssignedName { get; init; }
	private Label CluesCollected { get; init; }

	public Entry( string name, string assignedName, int cluesCollected, Color color )
	{
		Name.Text = name;
		AssignedName.Text = assignedName;
		CluesCollected.Text = cluesCollected.ToString();

		Name.Style.FontColor = color;
		AssignedName.Style.FontColor = color;
		CluesCollected.Style.FontColor = color;
	}
}
