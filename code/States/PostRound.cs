using System;
using Sandbox;

namespace Murder;

public enum WinType
{
	TimeUp,
	Elimination,
}

public partial class PostRound : BaseState
{
	[Net]
	public Role WinningRole { get; private set; }

	[Net]
	public WinType WinType { get; private set; }

	public override string Name { get; } = "Post";
	public override int Duration => Game.PostRoundTime;

	public PostRound() { }

	public PostRound( Role winningRole, WinType winType )
	{
		WinningRole = winningRole;
		WinType = winType;
	}

	public static void Load( Role winningTeam, WinType winType )
	{
		Game.Current.ForceStateChange( new PostRound( winningTeam, winType ) );
	}

	protected override void OnStart()
	{
		Game.Current.TotalRoundsPlayed++;
		// Event.Run( GameEvent.Round.End, WinningTeam, WinType );
	}

	protected override void OnTimeUp()
	{
		bool shouldChangeMap;

		shouldChangeMap = Game.Current.TotalRoundsPlayed >= Game.RoundLimit;
		shouldChangeMap |= Game.Current.RTVCount >= MathF.Round( Client.All.Count * Game.RTVThreshold );

		Game.Current.ChangeState( shouldChangeMap ? new MapSelectionState() : new PreRound() );
	}
}
