using Sandbox;

namespace Murder;

public static partial class GameEvent
{
	public static class Round
	{
		public const string Start = "murder.round.start";

		/// <summary>
		/// Occurs when the roles have been assigned and the round has started.
		/// </summary>
		public class StartAttribute : EventAttribute
		{
			public StartAttribute() : base( Start ) { }
		}

		public const string End = "murder.round.end";

		/// <summary>
		/// Occurs when a round has ended.
		/// <para><strong>Parameters:</strong></para>
		/// <para>The <see cref="Role"/> that won the round. </para>
		/// </summary>
		public class EndAttribute : EventAttribute
		{
			public EndAttribute() : base( End ) { }
		}
	}
}
