using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class RoundTimer : Panel
{
	private Label RoundName { get; init; }
	private Label Timer { get; init; }
	private Label SubText { get; init; }

	public override void Tick()
	{
		base.Tick();

		if ( Game.Current.State is null )
			return;

		if ( Local.Pawn is not Player player )
			return;

		RoundName.Text = Game.Current.State.Name;

		if ( Game.Current.State is WaitingState )
			Timer.Text = $"{Utils.MinimumPlayerCount()} / {Game.MinPlayers}";
		else
			Timer.Text = $"{Game.Current.State.TimeLeftFormatted}";

		if ( Game.Current.State is not InProgress inProgress )
		{
			SubText.SetClass( "show", false );
			return;
		}
	}
}
