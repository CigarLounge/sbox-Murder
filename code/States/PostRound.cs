using Sandbox;
using System;

namespace Murder;

public sealed partial class PostRound : GameState
{
	[Net] public Role WinningRole { get; private set; }
	public override int Duration => GameManager.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole )
	{
		WinningRole = winningRole;
	}

	protected override void OnStart()
	{
		GameManager.Instance.TotalRoundsPlayed++;

		if ( !Game.IsServer )
			return;

		Event.Run( GameEvent.Round.End, WinningRole );

		foreach ( var client in Game.Clients )
		{
			var player = (Player)client.Pawn;

			player.SendRole( To.Everyone );
		}

		RunEvent( WinningRole );
	}

	protected override void OnTimeUp()
	{
		bool shouldChangeMap;

		shouldChangeMap = GameManager.Instance.TotalRoundsPlayed >= GameManager.RoundLimit;
		shouldChangeMap |= GameManager.Instance.RTVCount >= MathF.Round( Game.Clients.Count * GameManager.RTVThreshold );

		if ( shouldChangeMap )
			GameManager.Instance.ForceStateChange( new MapSelectionState() );
		else
			GameManager.Instance.ChangeState( new GameplayState() );
	}

	[ClientRpc]
	public static void RunEvent( Role winningRole )
	{
		Event.Run( GameEvent.Round.End, winningRole );
	}
}

