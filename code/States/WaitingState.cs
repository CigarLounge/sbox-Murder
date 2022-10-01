using Sandbox;

namespace Murder;

public class WaitingState : BaseState
{
	public override void OnSecond()
	{
		if ( Host.IsServer && Utils.HasMinimumPlayers() )
			Game.Current.ForceStateChange( new GameplayState() );
	}

	protected override void OnStart()
	{
		if ( Game.Current.TotalRoundsPlayed == 0 )
			return;

		MapHandler.Cleanup();

		foreach ( var client in Client.All )
		{
			var player = (Player)client.Pawn;

			player.MakeSpectator();
		}
	}
}
