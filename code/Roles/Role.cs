using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public abstract class Role
{
	public static Bystander Bystander { get; private set; }
	public static Murderer Murderer { get; private set; }
	public static NoneRole None { get; private set; }

	private static readonly Dictionary<Type, HashSet<Player>> _players = new();
	public RoleInfo Info { get; private set; }
	public Color Color => Info.Color;
	public string Title => Info.Title;

	protected Role()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
	}

	public static void Init()
	{
		Bystander = new Bystander();
		Murderer = new Murderer();
		None = new NoneRole();
	}

	public static IEnumerable<Player> GetPlayers<T>() where T : Role
	{
		var type = typeof( T );

		if ( !_players.ContainsKey( type ) )
			_players.Add( type, new HashSet<Player>() );

		return _players[type];
	}

	[GameEvent.Player.RoleChanged]
	private static void OnPlayerRoleChanged( Player player, Role oldRole )
	{
		if ( oldRole is not null )
			_players[oldRole.GetType()].Remove( player );

		var newRole = player.Role;
		if ( newRole is not null )
		{
			if ( !_players.ContainsKey( newRole.GetType() ) )
				_players.Add( newRole.GetType(), new HashSet<Player>() );

			_players[newRole.GetType()].Add( player );
		}
	}

	public override bool Equals( object obj )
	{
		if ( ReferenceEquals( this, obj ) )
			return true;

		if ( obj is null )
			return false;

		throw new NotImplementedException();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}

#if SANDBOX && DEBUG
	[Event.Hotload]
	private void OnHotload()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
	}
#endif
}
