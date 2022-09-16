using Sandbox;
using System.Collections.Generic;

namespace Murder;

// TODO: Probably don't need any of this role stuff anymore.
public partial class Player
{
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

	/// <summary>
	/// Sends the role to the given target.
	/// </summary>
	/// <param name="to">The target. </param>
	public void SendRole( To to )
	{
		Host.AssertServer();

		foreach ( var client in to )
			ClientSetRole( To.Single( client ), Role.Info );
	}

	public void SetRole( RoleInfo roleInfo )
	{
		Role = TypeLibrary.Create<Role>( roleInfo.ClassName );
	}

	[ClientRpc]
	private void ClientSetRole( RoleInfo roleInfo )
	{
		SetRole( roleInfo );
	}
}
