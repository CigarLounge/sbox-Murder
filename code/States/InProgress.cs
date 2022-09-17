using Sandbox;
using System.Collections.Generic;

namespace Murder;

public partial class InProgress : BaseState
{
	public List<Player> AlivePlayers { get; set; }
	public List<Player> Spectators { get; set; }

	/// <summary>
	/// Unique case where InProgress has a seperate fake timer for Innocents.
	/// The real timer is only displayed to Traitors as it increments every player death during the round.
	/// </summary>
	[Net]
	public TimeUntil FakeTime { get; private set; }
	public string FakeTimeFormatted => FakeTime.Relative.TimerString();

	public override string Name { get; } = "In Progress";
	public override int Duration => Game.InProgressTime;

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		TimeLeft += Game.InProgressSecondsPerDeath;

		AlivePlayers.Remove( player );
		Spectators.Add( player );

		ChangeRoundIfOver();
	}

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		Spectators.Add( player );
	}

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		AlivePlayers.Remove( player );
		Spectators.Remove( player );

		ChangeRoundIfOver();
	}

	protected override void OnStart()
	{
		Event.Run( GameEvent.Round.Start );

		if ( !Host.IsServer )
			return;

		FakeTime = TimeLeft;

		foreach ( var ent in Entity.All )
		{
			if ( ent is Corpse corpse )
				corpse.Delete();
		}
	}

	protected override void OnTimeUp()
	{
		PostRound.Load( Role.Murderer, WinType.TimeUp );
	}

	private Role IsRoundOver()
	{
		// TODO: Fix.
		// List<Role> aliveRoles = new();

		// foreach ( var player in AlivePlayers )
		// {
		// 	if ( !aliveTeams.Contains( player.Role ) )
		// 		aliveTeams.Add( player.Team );
		// }

		// if ( aliveTeams.Count == 0 )
		// 	return Team.None;

		// return aliveTeams.Count == 1 ? aliveTeams[0] : Team.None;
		return Role.Murderer;
	}

	public override void OnSecond()
	{
		if ( !Host.IsServer )
			return;

		if ( Game.PreventWin )
			TimeLeft += 1f;

		if ( TimeLeft )
			OnTimeUp();

		// if ( !Utils.HasMinimumPlayers() && IsRoundOver() == Team.None )
		// 	Game.Current.ForceStateChange( new WaitingState() );
	}

	private bool ChangeRoundIfOver()
	{
		var result = IsRoundOver();

		if ( result != Role.Murderer && !Game.PreventWin )
		{
			PostRound.Load( Role.Murderer, WinType.Elimination );
			return true;
		}

		return false;
	}

	[GameEvent.Player.RoleChanged]
	private static void OnPlayerRoleChange( Player player, Role oldRole )
	{
		if ( !Host.IsServer )
			return;

		if ( Game.Current.State is InProgress inProgress )
			inProgress.ChangeRoundIfOver();
	}
}
