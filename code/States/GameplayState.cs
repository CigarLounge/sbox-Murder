using Sandbox;
using Sandbox.Diagnostics;
using System.Collections.Generic;

namespace Murder;

public sealed partial class GameplayState : GameState
{
	public override int Duration => 5;
	private readonly List<Player> _alivePlayers = new();
	private TimeUntil _timeUntilNextClue = GameManager.ClueSpawnRate + 5;

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

		Player.Names.Shuffle();
		for ( var i = 0; i < _alivePlayers.Count; i++ )
		{
			var player = _alivePlayers[i];
			player.BystanderName = Player.Names[i];
			player.Color = Color.FromBytes( Game.Random.Int( 0, 255 ), Game.Random.Int( 0, 255 ), Game.Random.Int( 0, 255 ) );
			player.ColoredClothing.RenderColor = player.Color;
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
