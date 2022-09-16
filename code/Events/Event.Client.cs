using Sandbox;

namespace Murder;

public static partial class GameEvent
{
	public static class Client
	{
		public const string Joined = "murder.client.joined";

		/// <summary>
		/// Called everytime a player joins.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Sandbox.Client"/> that joined.</para>
		/// </summary>
		public class JoinedAttribute : EventAttribute
		{
			public JoinedAttribute() : base( Joined ) { }
		}

		public const string Disconnected = "murder.client.disconnected";

		/// <summary>
		/// Called everytime a player leaves the game.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Sandbox.Client"/> that disconnected.</para>
		/// </summary>
		public class DisconnectedAttribute : EventAttribute
		{
			public DisconnectedAttribute() : base( Disconnected ) { }
		}
	}
}
