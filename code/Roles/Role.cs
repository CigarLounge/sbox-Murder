using System.Collections.Generic;

namespace Murder;

public abstract class Role
{
	public static readonly Bystander Bystander = new();
	public static readonly Murderer Murderer = new();
	public static readonly NoneRole None = new();

	public virtual byte Ident => 255;
	public virtual Color Color { get; }
	public HashSet<Player> Players { get; init; }
	public virtual string Title { get; }

	protected Role() { }

	[GameEvent.Player.RoleChanged]
	private static void OnPlayerRoleChanged( Player player, Role oldRole )
	{
		oldRole?.Players.Remove( player );
		player.Role?.Players.Add( player );
	}
}
