using Sandbox;

namespace Murder;

public static class MapHandler
{
	public static void Cleanup()
	{
		Game.ResetMap( System.Array.Empty<Entity>() );
		Decal.Clear( true, true );
	}
}
