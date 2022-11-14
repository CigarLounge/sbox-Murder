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

	public PostRound( Role winningRole, List<Player> players )
	{
		WinningRole = winningRole;

		List<UI.PostRoundPopup.PostRoundData.PlayerData> playerData = new();
		players.ForEach( ( bystander ) =>
		{
			playerData.Add( new UI.PostRoundPopup.PostRoundData.PlayerData
			{
				Name = bystander.Client.Name,
				AssignedName = bystander.BystanderName,
				CluesCollected = bystander.CluesCollected,
				Color = bystander.Color,
				Role = bystander.Role
			} );
		} );

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
