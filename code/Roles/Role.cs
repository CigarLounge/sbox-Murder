using System.Collections.Generic;

namespace Murder;

public enum Role
{
	None,
	Bystander,
	Murderer
}

public static class RoleExtensions
{
	private static readonly HashSet<Player>[] _players;

	static RoleExtensions()
	{
		_players = new HashSet<Player>[3];
		_players[0] = new();
		_players[1] = new();
		_players[2] = new();
	}

	public static Color GetColor( this Role role )
	{
		return role switch
		{
			Role.None => Color.White,
			Role.Bystander => new Color32( 85, 212, 255 ),
			Role.Murderer => new Color32( 255, 56, 56 ),
			_ => Color.White
		};
	}

	public static IEnumerable<Player> GetPlayers( this Role role )
	{
		return _players[(int)role];
	}

	public static string GetTitle( this Role role )
	{
		return role switch
		{
			Role.None => "None",
			Role.Bystander => "Bystander",
			Role.Murderer => "Murderer",
			_ => string.Empty
		};
	}

	[GameEvent.Player.RoleChanged]
	private static void OnPlayerRoleChanged( Player player, Role oldRole )
	{
		_players[(int)oldRole].Remove( player );
		_players[(int)player.Role].Add( player );
	}
}
