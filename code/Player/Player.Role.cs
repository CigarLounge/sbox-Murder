using Sandbox;

namespace Murder;

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

		ClientSetRole( to, Role.Ident );
	}

	public void SetRole( byte ident )
	{
		Role = ident switch
		{
			0 => Role.None,
			1 => Role.Bystander,
			2 => Role.Murderer,
			_ => null,
		};
	}

	[ClientRpc]
	private void ClientSetRole( byte ident )
	{
		SetRole( ident );
	}
}
