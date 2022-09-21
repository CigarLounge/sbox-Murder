using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public class PreRound : BaseState
{
	public override string Name { get; } = "Preparing";
	public override int Duration => Game.Current.TotalRoundsPlayed == 0 ? Game.PreRoundTime * 2 : Game.PreRoundTime;

	public override void OnPlayerSpawned( Player player )
	{
		base.OnPlayerSpawned( player );

		// player.Inventory.Add( new Hands() );
	}

	public override void OnPlayerJoin( Player player )
	{
		base.OnPlayerJoin( player );

		player.Respawn();
	}

	public override void OnPlayerKilled( Player player )
	{
		base.OnPlayerKilled( player );

		StartRespawnTimer( player );
	}

	protected override void OnStart()
	{
		MapHandler.Cleanup();

		if ( !Host.IsServer )
			return;

		foreach ( var client in Client.All )
		{
			var player = client.Pawn as Player;
			player.Respawn();
		}
	}

	protected override void OnTimeUp()
	{
		List<Player> players = new();
		List<Player> spectators = new();

		foreach ( var client in Client.All )
		{
			var player = client.Pawn as Player;

			if ( player.IsForcedSpectator )
			{
				player.MakeSpectator( false );
				spectators.Add( player );

				continue;
			}

			if ( player.IsAlive() )
				player.Health = Player.MaxHealth;
			else
				player.Respawn();

			players.Add( player );
		}

		AssignRoles( players );

		Game.Current.ChangeState( new InProgress
		{
			AlivePlayers = players,
			Spectators = spectators,
		} );
	}

	private static void AssignRoles( List<Player> players )
	{
		players.Shuffle();

		var index = 0;
		foreach ( var player in players )
		{
			player.Role = Role.Bystander;
			player.AssignedName = Game.Names[index++];
			player.AssignedColor = Color.FromBytes( Rand.Int( 0, 255 ), Rand.Int( 0, 255 ), Rand.Int( 0, 255 ) );
			player.ColoredClothing.RenderColor = player.AssignedColor;

			if ( index > Game.Names.Count )
				index = 0;
		}

		var murderer = Rand.FromList( players );
		murderer.Role = Role.Murderer;
		murderer.SetCarriable( new Knife() );

		var detective = Rand.FromList( players );
		while ( detective == murderer ) // this is shit
			detective = Rand.FromList( players );

		detective.SendRole( To.Everyone );
		detective.SetCarriable( new Revolver() );
	}
}
