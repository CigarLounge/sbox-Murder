using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public partial class PostRoundPopup : Panel
{
	private Panel Players { get; init; }
	private Label WinningText { get; init; }
	private Label Murderer { get; init; }

	[ClientRpc]
	public static void Display( Role winningRole, List<Player> murderers, List<Player> bystanders )
	{
		Log.Info( winningRole );
		Log.Info( murderers.Count );
		Log.Info( bystanders.Count );
	}
}
