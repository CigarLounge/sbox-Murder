using Sandbox;
using System.Collections.Generic;

namespace Murder;

public sealed partial class GameplayState : BaseState
{
	private readonly List<Player> _alivePlayers = new();

	// This is used for freeze time.
	public override int Duration => 5;

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
		MapHandler.Cleanup();

		if ( !Host.IsServer )
			return;

		foreach ( var client in Client.All )
		{
			var player = (Player)client.Pawn;

			player.Respawn();

			if ( !player.IsForcedSpectator )
				_alivePlayers.Add( player );
		}

		AssignRoles();

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
		if ( !Host.IsServer )
			return;

		if ( !Utils.HasMinimumPlayers() && IsRoundOver() )
			Game.Current.ForceStateChange( new WaitingState() );
	}

	private void AssignRoles()
	{
		_alivePlayers.Shuffle();

		var murderer = _alivePlayers[0];
		murderer.Role = Role.Murderer;
		murderer.Inventory.Add( new Knife(), true );

		var detective = _alivePlayers[1];
		detective.Role = Role.Bystander;
		detective.Inventory.Add( new Revolver(), true );

		for ( var i = 2; i < _alivePlayers.Count; i++ )
			_alivePlayers[i].Role = Role.Bystander;

		Player.Names.Shuffle();
		for ( var i = 0; i < _alivePlayers.Count; i++ )
		{
			var player = _alivePlayers[i];

			player.BystanderName = Player.Names[i];
			player.Color = Color.FromBytes( Rand.Int( 0, 255 ), Rand.Int( 0, 255 ), Rand.Int( 0, 255 ) );
			player.ColoredClothing.RenderColor = player.Color;
			player.Inventory.Add( new Holster(), true );
		}
	}

	private bool ChangeRoundIfOver()
	{
		if ( IsRoundOver() && !Game.PreventWin )
		{
			Game.Current.ForceStateChange( new PostRound( _alivePlayers[0].Role ) );
			return true;
		}

		return false;
	}

	[ClientRpc]
	public static void RunEvent()
	{
		Assert.NotNull( Game.Current.State as GameplayState );
		Event.Run( GameEvent.Round.Start );
	}
}
