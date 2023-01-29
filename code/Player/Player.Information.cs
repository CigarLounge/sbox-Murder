using Sandbox;
using System.Collections.Generic;

namespace Murder;

public partial class Player
{
	public static readonly List<string> Names = new()
	{
		"Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India",
		"Juliett", "Kilo", "Lima", "Miko", "November", "Oscar", "Papa", "Quebec", "Romeo",
		"Sierra", "Tango", "Uniform", "Victor", "Whiskey", "X-ray", "Yankee", "Zulu"
	};

	[Net] public string BystanderName { get; set; }
	[Net] public Color Color { get; set; }
	[Net] public int CluesCollected { get; set; }
	public Corpse Corpse { get; internal set; }
	public bool IsIdentityHidden => this.IsAlive() && GameState.Current is GameplayState;

	private Role _role;
	public Role Role
	{
		get => _role;
		set
		{
			if ( _role == value )
				return;

			var oldRole = _role;
			_role = value;

			// Always send the role to this player's client
			if ( Game.IsServer )
				SendRole( To.Single( this ) );

			Event.Run( GameEvent.Player.RoleChanged, this, oldRole );
		}
	}

	public void ResetInformation()
	{
		BystanderName = null;
		CluesCollected = 0;
		Color = default;
		Corpse = null;
		Role = Role.None;
	}

	/// <summary>
	/// Sends the role to the given target.
	/// </summary>
	/// <param name="to">The target. </param>
	public void SendRole( To to )
	{
		Game.AssertServer();

		ClientSetRole( to, Role );
	}

	[ClientRpc]
	private void ClientSetRole( Role role )
	{
		Role = role;
	}
}
