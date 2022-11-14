using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public partial class PostRound : BaseState
{
	[Net]
	public Role WinningRole { get; private set; }

	public override int FreezeDuration => Game.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole, List<Player> murderers, List<Player> bystanders )
	{
		WinningRole = winningRole;
		UI.PostRoundPopup.Display( winningRole, murderers, bystanders );
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
