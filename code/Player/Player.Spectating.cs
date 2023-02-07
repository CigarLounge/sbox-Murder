using Sandbox;

namespace Murder;

public partial class Player
{
	[Net]
	public bool IsForcedSpectator { get; private set; }

	[ConCmd.Server]
	public static void ToggleForceSpectator()
	{
		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		player.IsForcedSpectator = !player.IsForcedSpectator;

		if ( player.IsForcedSpectator && player.IsAlive() )
			player.Kill();
	}
}
