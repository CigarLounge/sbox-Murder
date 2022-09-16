using Sandbox;

namespace Murder;

public static class MapHandler
{
	public static void Cleanup()
	{
		Map.Reset( Sandbox.Game.DefaultCleanupFilter );
		Decal.Clear( true, true );
	}
}
