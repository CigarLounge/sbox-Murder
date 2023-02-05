using Sandbox;

namespace Murder;

public class WaitingState : GameState
{
	public override int Duration => 10;

	public override void OnSecond()
	{
		var hasMinimumPlayers = GameManager.HasMinimumPlayers();

		if ( hasMinimumPlayers )
		{
			if ( Game.IsServer && TimeLeft )
				GameManager.Instance.ForceStateChange( new GameplayState() );
			else if ( Game.IsClient )
				UI.TextChat.AddInfo( $"The round starts in {TimeLeftFormatted}" );
		}
		else
			TimeLeft = Duration;
	}

	protected override void OnStart()
	{
		if ( GameManager.Instance.TotalRoundsPlayed == 0 )
			return;

		Map.Cleanup();

		foreach ( var client in Game.Clients )
		{
			var player = (Player)client.Pawn;

			player.MakeSpectator();
		}
	}
}
