using Sandbox;
using System.Collections.Generic;

namespace Murder;

public partial class InProgress : BaseState
{
	public List<Player> AlivePlayers { get; set; }
	public List<Player> Spectators { get; set; }

	public override string Name { get; } = "In Progress";
	public override int Duration => Game.InProgressTime;

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

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

		foreach ( var ent in Entity.All )
		{
			if ( ent is Corpse corpse )
				corpse.Delete();
		}
	}

	private Role IsRoundOver()
	{
		List<Role> aliveRoles = new();

		foreach ( var player in AlivePlayers )
		{
			if ( !aliveRoles.Contains( player.Role ) )
				aliveRoles.Add( player.Role );
		}

		if ( aliveRoles.Count == 0 )
			return Role.None;

		return aliveRoles.Count == 1 ? aliveRoles[0] : Role.None;
	}

	public override void OnSecond()
	{
		if ( !Host.IsServer )
			return;

		if ( !Utils.HasMinimumPlayers() && IsRoundOver() == Role.None )
			Game.Current.ForceStateChange( new WaitingState() );
	}

	private bool ChangeRoundIfOver()
	{
		var result = IsRoundOver();

		if ( result != Role.None && !Game.PreventWin )
		{
			PostRound.Load( result );
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
