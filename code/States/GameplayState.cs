using Sandbox;
using System.Collections.Generic;

namespace Murder;

public sealed partial class GameplayState : BaseState
{
	public override int FreezeDuration => 5;
	private readonly List<Player> _alivePlayers = new();
	private readonly List<Player> _bystanders = new();
	private readonly List<Player> _murderers = new();

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

	private Role IsRoundOver()
	{
		List<Role> aliveRoles = new();

		foreach ( var player in _alivePlayers )
			if ( !aliveRoles.Contains( player.Role ) )
				aliveRoles.Add( player.Role );

		if ( aliveRoles.IsNullOrEmpty() )
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

	private void AssignRoles()
	{
		_alivePlayers.Shuffle();

		var murderer = _alivePlayers[0];
		murderer.Role = Role.Murderer;
		murderer.Inventory.Add( new Knife(), true );
		_murderers.Add( murderer );

		var detective = _alivePlayers[1];
		detective.Role = Role.Bystander;
		detective.Inventory.Add( new Revolver(), true );
		_bystanders.Add( detective );

		Player.Names.Shuffle();
		for ( var i = 0; i < _alivePlayers.Count; i++ )
		{
			var player = _alivePlayers[i];

			if ( player.Role == Role.None )
			{
				_alivePlayers[i].Role = Role.Bystander;
				_bystanders.Add( detective );
			}

			player.BystanderName = Player.Names[i];
			player.Color = Color.FromBytes( Rand.Int( 0, 255 ), Rand.Int( 0, 255 ), Rand.Int( 0, 255 ) );
			player.ColoredClothing.RenderColor = player.Color;
			player.Inventory.Add( new Holster(), true );
		}
	}

	private void ChangeRoundIfOver()
	{
		var result = IsRoundOver();
		if ( result != Role.None && !Game.PreventWin )
			Game.Current.ForceStateChange( new PostRound( result, _murderers, _bystanders ) );
	}

	[ClientRpc]
	public static void RunEvent()
	{
		Assert.NotNull( Game.Current.State as GameplayState );
		Event.Run( GameEvent.Round.Start );
	}
}
