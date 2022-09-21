using Sandbox;

namespace Murder;

public partial class Player
{
	[Net]
	public string AssignedName { get; set; }

	[Net]
	public Color32 AssignedColor { get; set; }

	[Net]
	public int CluesCollected { get; set; }

	[Net]
	public string SteamName { get; private set; }

	public Corpse Corpse { get; set; }

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
			if ( IsServer )
				SendRole( To.Single( this ) );

			Event.Run( GameEvent.Player.RoleChanged, this, oldRole );
		}
	}

	public void ResetInformation()
	{
		AssignedName = null;
		AssignedColor = default;
		CluesCollected = 0;
		Role = Role.None;
	}

	/// <summary>
	/// Sends the role to the given target.
	/// </summary>
	/// <param name="to">The target. </param>
	public void SendRole( To to )
	{
		Host.AssertServer();

		ClientSetRole( to, Role );
	}

	[ClientRpc]
	private void ClientSetRole( Role role )
	{
		Role = role;
	}
}
