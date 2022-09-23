using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public sealed class RoundStartPopup : Panel
{
	private static RoundStartPopup _instance;

	private Label Title { get; init; }
	private Label Subtitle { get; init; }
	private Label Help { get; init; }

	public RoundStartPopup()
	{
		_instance = this;

		var player = (Player)Local.Pawn;

		Style.FontColor = player.Role.GetColor();
		Title.Text = $"You are a {player.Role.GetTitle()}";
		Subtitle.Enabled( player.Carriable is Revolver );

		Sound.FromScreen( "scream" );
	}

	public override void Tick()
	{
		if ( Game.Current.State.TimeLeft )
			Delete();
	}

	[GameEvent.Round.Start]
	private static void OnRoundStart()
	{
		if ( !Host.IsClient )
			return;

		if ( Game.Current.State.TimeLeft )
			return;

		var player = (Player)Local.Pawn;

		if ( player.Role == Role.None )
			return;

		Local.Hud.AddChild<RoundStartPopup>();
	}
}
