using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public static class Utils
{
	public static List<Player> GetPlayersWhere( Func<Player, bool> predicate )
	{
		List<Player> players = new();
		foreach ( var client in Game.Clients )
			if ( client.Pawn is Player player && predicate.Invoke( player ) )
				players.Add( player );

		return players;
	}

	public static List<IClient> GetClientsWhere( Func<Player, bool> predicate )
	{
		List<IClient> clients = new();
		foreach ( var client in Game.Clients )
			if ( client.Pawn is Player player && predicate.Invoke( player ) )
				clients.Add( client );

		return clients;
	}
}
