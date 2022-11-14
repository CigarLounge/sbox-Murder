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

	public PostRound( Role winningRole, List<Player> murderers, List<Player> bystanders )
	{
		WinningRole = winningRole;

		List<UI.PostRoundPopup.PlayerData> murdererData = new();
		murderers.ForEach( ( murderer ) =>
		{
			murdererData.Add( new UI.PostRoundPopup.PlayerData
			{
				Name = murderer.Client.Name,
				AssignedName = murderer.BystanderName,
				CluesCollected = murderer.CluesCollected,
				Color = murderer.Color
			} );
		} );

		List<UI.PostRoundPopup.PlayerData> bystanderData = new();
		bystanders.ForEach( ( bystander ) =>
		{
			bystanderData.Add( new UI.PostRoundPopup.PlayerData
			{
				Name = bystander.Client.Name,
				AssignedName = bystander.BystanderName,
				CluesCollected = bystander.CluesCollected,
				Color = bystander.Color
			} );
		} );

		UI.PostRoundPopup.Display( WinningRole, Utils.Serialize( murdererData.ToArray() ), Utils.Serialize( bystanderData.ToArray() ) );
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
