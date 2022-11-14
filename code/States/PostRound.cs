using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public partial class PostRound : BaseState
{
	[Net]
	public Role WinningRole { get; private set; }

	public override int Duration => Game.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole )
	{
		WinningRole = winningRole;

		List<UI.PostRoundPopup.PostRoundData.PlayerData> playerData = new();
		foreach ( var client in Client.All )
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
		} ) );
	}

	protected override void OnStart()
	{
		Game.Current.TotalRoundsPlayed++;

		Event.Run( GameEvent.Round.End, WinningRole );
	}

	protected override void OnTimeUp()
	{
		bool shouldChangeMap;

		shouldChangeMap = Game.Current.TotalRoundsPlayed >= Game.RoundLimit;
		shouldChangeMap |= Game.Current.RTVCount >= MathF.Round( Client.All.Count * Game.RTVThreshold );

		if ( shouldChangeMap )
			Game.Current.ForceStateChange( new MapSelectionState() );
		else
			Game.Current.ChangeState( new GameplayState() );
	}
}
