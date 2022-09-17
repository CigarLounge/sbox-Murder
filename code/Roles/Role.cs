using Sandbox;
using System;
using System.Collections.Generic;

namespace Murder;

public abstract class Role : IEquatable<Role>, IEquatable<string>
{
	public static Bystander Bystander { get; private set; }
	public static Murderer Murderer { get; private set; }

	private static readonly Dictionary<Type, HashSet<Player>> _players = new();
	public RoleInfo Info { get; private set; }
	public Color Color => Info.Color;
	public string Title => Info.Title;

	public Role()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
	}

	public static void Init()
	{
		Bystander = new Bystander();
		Murderer = new Murderer();
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

	public static bool operator ==( Role left, Role right )
	{
		if ( left is null )
		{
			if ( right is null )
				return true;

			return false;
		}

		return left.Equals( right );
	}
	public static bool operator !=( Role left, Role right ) => !(left == right);

	public static bool operator ==( Role left, string right )
	{
		if ( left is null || right is null )
			return false;

		return left.Equals( right );
	}
	public static bool operator !=( Role left, string right ) => !(left == right);

	public bool Equals( Role other )
	{
		if ( other is null )
			return false;

		if ( ReferenceEquals( this, other ) )
			return true;

		return Info == other.Info;
	}

	public bool Equals( string other )
	{
		if ( Info.Title.Equals( other, StringComparison.OrdinalIgnoreCase ) )
			return true;

		if ( Info.ClassName.Equals( other, StringComparison.OrdinalIgnoreCase ) )
			return true;

		return false;
	}

	public override bool Equals( object obj ) => Equals( obj as Role );

	public override int GetHashCode() => Info.ResourceId.GetHashCode();

#if SANDBOX && DEBUG
	[Event.Hotload]
	private void OnHotload()
	{
		Info = GameResource.GetInfo<RoleInfo>( GetType() );
	}
#endif
}
