using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Murder.UI;

public partial class TabMenu : Panel
{
	public static TabMenu Instance;

	private readonly Scoreboard _scoreboard;
	private readonly Button _muteButton;

	public TabMenu()
	{
		Instance = this;

		StyleSheet.Load( "/UI/General/TabMenu/TabMenu.scss" );

		var scoreboardButtons = new Panel();
		scoreboardButtons.AddClass( "spacing" );
		_muteButton = scoreboardButtons.Add.ButtonWithIcon( "Mute Alive Players", "volume_up", string.Empty, Player.ToggleMute );
		_scoreboard = new Scoreboard( this, scoreboardButtons );

		_scoreboard.EnableFade( true );
	}

	public override void Tick()
	{
		if ( Local.Client.Pawn is not Player player )
			return;

		_muteButton.Enabled( !player.IsAlive() );
		if ( !_muteButton.IsEnabled() )
			return;

		switch ( player.MuteFilter )
		{
			case MuteFilter.None:
				_muteButton.Text = "Mute Alive Players";
				_muteButton.Icon = "volume_up";
				break;
			case MuteFilter.AlivePlayers:
				_muteButton.Text = "Mute Spectators";
				_muteButton.Icon = "volume_off";
				break;
			case MuteFilter.Spectators:
				_muteButton.Text = "Mute All Players";
				_muteButton.Icon = "volume_off";
				break;
			case MuteFilter.All:
				_muteButton.Text = "Unmute Players";
				_muteButton.Icon = "volume_off";
				break;
		}
	}

	[Event.BuildInput]
	private void MenuInput( InputBuilder input )
	{
		SetClass( "show", input.Down( InputButton.Score ) );
	}
}
