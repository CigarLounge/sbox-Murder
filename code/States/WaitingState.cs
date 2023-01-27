using Sandbox;

namespace Murder;

public class WaitingState : GameState
{
	public override void OnSecond()
	{
		if ( Game.IsServer && GameManager.HasMinimumPlayers() )
			GameManager.Instance.ForceStateChange( new GameplayState() );
	}

	protected override void OnStart()
	{
		if ( GameManager.Instance.TotalRoundsPlayed == 0 )
			return;

		MapHandler.Cleanup();

		foreach ( var client in Game.Clients )
		{
			var player = (Player)client.Pawn;

			player.MakeSpectator();
		}
	}
}
