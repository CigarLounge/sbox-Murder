/*using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public partial class PostRoundPopup : Panel
{
	public class PostRoundData
	{
		public Role WinningRole { get; set; }
		public List<PlayerData> Players { get; set; }

		public class PlayerData
		{
			public string Name { get; set; }
			public string AssignedName { get; set; }
			public int CluesCollected { get; set; }
			public Color Color { get; set; }
			public Role Role { get; set; }
		}
	}

	private Panel Players { get; init; }
	private Label WinningText { get; init; }
	private Label Murderer { get; init; }

	[ClientRpc]
	public static void Display( byte[] data )
	{
		var postRoundData = Utils.Deserialize<PostRoundData>( data );

		var popup = Local.Hud.AddChild<PostRoundPopup>();
		popup.WinningText.Text = $"{postRoundData.WinningRole.GetTitle()}s win!";
		popup.WinningText.Style.FontColor = postRoundData.WinningRole.GetColor();

		var murderersText = string.Empty;
		postRoundData.Players.ForEach( ( player ) =>
		{
			if ( player.Role == Role.Bystander )
				popup.Players.AddChild( new Entry( player.Name, player.AssignedName, player.CluesCollected, player.Color ) );
			else if ( player.Role == Role.Murderer )
				murderersText += $"{player.Name} ";
		} );
		popup.Murderer.Text = murderersText;
	}

	[Event.Entity.PostCleanup]
	public void PostCleanup()
	{
		Delete();
	}
}
*/
