using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public partial class PostRound : GameState
{
	[Net] public Role WinningRole { get; private set; }
	public override int Duration => GameManager.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole )
	{
		WinningRole = winningRole;

		/*List<UI.PostRoundPopup.PostRoundData.PlayerData> playerData = new();

		foreach ( var client in Game.Clients )
		{
			if ( client.Pawn is not Player player )
				return;

			playerData.Add( new UI.PostRoundPopup.PostRoundData.PlayerData
			{
				Name = player.Client.Name,
				AssignedName = player.BystanderName,
				CluesCollected = player.CluesCollected,
				Color = player.Color,
				Role = player.Role
			} );
		}

		UI.PostRoundPopup.Display( Utils.Serialize( new UI.PostRoundPopup.PostRoundData
		{
			WinningRole = WinningRole,
			Players = playerData
		} ) );*/
	}

	protected override void OnStart()
	{
		GameManager.Instance.TotalRoundsPlayed++;

		Event.Run( GameEvent.Round.End, WinningRole );
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
}

