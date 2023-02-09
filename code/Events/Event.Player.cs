using Sandbox;

namespace Murder;

public static partial class GameEvent
{
	public static class Player
	{
		public const string Killed = "murder.player.killed";

		/// <summary>
		/// Occurs when a player dies.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Murder.Player"/> who died.</para>
		/// </summary>
		public class KilledAttribute : EventAttribute
		{
			public KilledAttribute() : base( Killed ) { }
		}

		public const string Spawned = "murder.player.spawned";

		/// <summary>
		/// Occurs when a player spawns.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Murder.Player"/> who spawned.</para>
		/// </summary>
		public class SpawnedAttribute : EventAttribute
		{
			public SpawnedAttribute() : base( Spawned ) { }
		}
	}
}
