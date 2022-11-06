using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Entry : Panel
{
	private Label Name { get; init; }
	private Label AssignedName { get; init; }
	private Label CluesCollected { get; init; }

	public Entry( string name, string assignedName, int cluesCollected )
	{
		Name.Text = name;
		AssignedName.Text = assignedName;
		CluesCollected.Text = cluesCollected.ToString();
	}
}
