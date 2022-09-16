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

		public const string RoleChanged = "murder.player.role-changed";

		/// <summary>
		/// Occurs when a player's role has changed.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Murder.Player"/> whose role has changed. </para>
		/// <para><see cref="Role"/> their old role. </para>
		/// </summary>
		public class RoleChangedAttribute : EventAttribute
		{
			public RoleChangedAttribute() : base( RoleChanged ) { }
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

		public const string StatusChanged = "murder.player.status-changed";

		/// <summary>
		/// Occurs when a player's status has changed.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Murder.Player"/> whose status has changed.</para>
		/// <para>The old <see cref="PlayerStatus"/>.</para>
		/// </summary>
		public class StatusChangedAttribute : EventAttribute
		{
			public StatusChangedAttribute() : base( StatusChanged ) { }
		}

		public const string TookDamage = "murder.player.took-damage";

		/// <summary>
		/// Occurs when a player takes damage.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Murder.Player"/> who took damage.</para>
		/// </summary>
		public class TookDamageAttribute : EventAttribute
		{
			public TookDamageAttribute() : base( TookDamage ) { }
		}
	}
}
