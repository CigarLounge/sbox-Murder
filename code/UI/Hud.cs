using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[StyleSheet( "/UI/Hud.scss" )]
public class Hud : RootPanel
{
	/// <summary>
	/// The player that we're currently displaying info about through UI elements.
	/// </summary>
	public static Player DisplayedPlayer => Spectating.Player.IsValid() ? Spectating.Player : (Player)Game.LocalPawn;

	public Hud()
	{
		Game.RootPanel?.Delete( true );
		Game.RootPanel = this;

		AddChild<Scoreboard>();
		AddChild<HintDisplay>();
		AddChild<PlayerInfo>();
		AddChild<CarriableDisplay>();
		AddChild<Crosshair>();
		AddChild<TextChat>();
		AddChild<VoiceChat>();
		AddChild<SpectatingPrompts>();
	}
}
