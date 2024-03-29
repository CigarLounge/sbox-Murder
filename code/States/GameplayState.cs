using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

public sealed partial class GameplayState : GameState
{
	public override int Duration => 5;
	private readonly List<Player> _alivePlayers = new();
	private TimeUntil _timeUntilNextClue = Clue.SpawnRate + 5;

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		_alivePlayers.Remove( player );

		ChangeRoundIfOver();
	}

	public override void OnPlayerLeave( Player player )
	{
		base.OnPlayerLeave( player );

		_alivePlayers.Remove( player );

		ChangeRoundIfOver();
	}

	protected override void OnStart()
	{
		Map.Cleanup();

		if ( Game.IsClient && Spectating.IsForced )
			UI.TextChat.AddInfo( "You are currently spectating, disable spectating using the scoreboard." );

		if ( !Game.IsServer )
			return;

		foreach ( var client in Game.Clients )
		{
			var player = (Player)client.Pawn;

			player.Respawn();

			if ( !player.IsForcedSpectator )
				_alivePlayers.Add( player );
		}

		AssignRoles();
		Map.Clues.Shuffle();

		Event.Run( GameEvent.Round.Start );
		RunEvent();
	}

	private bool IsRoundOver()
	{
		var aliveRoles = new bool[2];

		foreach ( var player in _alivePlayers )
			aliveRoles[(int)player.Role - 1] = true;

		return aliveRoles[0] ^ aliveRoles[1];
	}

	public override void OnSecond()
	{
		if ( !Game.IsServer )
			return;

		if ( _timeUntilNextClue )
		{
			Map.SpawnClue();
			_timeUntilNextClue = Clue.SpawnRate;
		}

		if ( !GameManager.HasMinimumPlayers() && IsRoundOver() )
			GameManager.Instance.ForceStateChange( new WaitingState() );
	}

	private void AssignRoles()
	{
		_alivePlayers.Shuffle();

		var murderer = _alivePlayers[0];
		murderer.Role = Role.Murderer;
		murderer.SetCarriable( new Knife() );

		var detective = _alivePlayers[1];
		detective.Role = Role.Bystander;
		detective.SetCarriable( new Revolver() );

		for ( var i = 2; i < _alivePlayers.Count; i++ )
			_alivePlayers[i].Role = Role.Bystander;

		var indices = Enumerable.Range( 0, GameManager.Instance.PlayerNames.Count ).ToArray();
		indices.Shuffle();

		for ( int i = 0, j = 0; i < _alivePlayers.Count; i++, j++ )
		{
			if ( j >= GameManager.Instance.PlayerNames.Count )
				j = 0;

			var player = _alivePlayers[i];

			player.NameIndex = indices[j];
			player.Color = Color.FromBytes( Game.Random.Int( 0, 255 ), Game.Random.Int( 0, 255 ), Game.Random.Int( 0, 255 ) );
		}
	}

	private bool ChangeRoundIfOver()
	{
		if ( IsRoundOver() && !GameManager.PreventWin )
		{
			GameManager.Instance.ForceStateChange( new PostRound( _alivePlayers[0].Role ) );
			return true;
		}

		return false;
	}

	[ClientRpc]
	public static void RunEvent()
	{
		Assert.NotNull( Current as GameplayState );
		Event.Run( GameEvent.Round.Start );
	}
}
