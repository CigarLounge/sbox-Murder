using System;
using Sandbox;

namespace Murder;

public partial class PostRound : BaseState
{
	[Net]
	public Role WinningRole { get; private set; }

	public override string Name { get; } = "Post";
	public override int Duration => Game.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole )
	{
		WinningRole = winningRole;
	}

	public static void Load( Role winningTeam )
	{
		Game.Current.ForceStateChange( new PostRound( winningTeam ) );
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

		Game.Current.ChangeState( shouldChangeMap ? new MapSelectionState() : new PreRound() );
	}
}
