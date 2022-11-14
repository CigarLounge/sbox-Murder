using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public partial class PostRoundPopup : Panel
{
	public class PlayerData
	{
		public string Name { get; set; }
		public string AssignedName { get; set; }
		public int CluesCollected { get; set; }
		public Color Color { get; set; }
	}

	private Panel Players { get; init; }
	private Label WinningText { get; init; }
	private Label Murderer { get; init; }

	[ClientRpc]
	public static void Display( Role winningRole, byte[] murderers, byte[] bystanders )
	{
		var popup = Local.Hud.AddChild<PostRoundPopup>();
		popup.WinningText.Text = $"{winningRole.GetTitle()}s win!";
		popup.WinningText.Style.FontColor = winningRole.GetColor();

		var murdererData = Utils.Deserialize<List<PlayerData>>( murderers );
		var bystanderData = Utils.Deserialize<List<PlayerData>>( bystanders );

		var murderersText = string.Empty;
		murdererData.ForEach( ( murderer ) => { murderersText += $"{murderer.Name} "; } );
		popup.Murderer.Text = murderersText;

		bystanderData.ForEach( ( bystander ) =>
		{
			popup.Players.AddChild( new Entry( bystander.Name, bystander.AssignedName, bystander.CluesCollected, bystander.Color ) );
		} );
	}

	[Event.Entity.PostCleanup]
	public void PostCleanup()
	{
		Delete();
	}
}
