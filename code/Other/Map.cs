using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

internal static class Map
{
	internal static List<Clue> Clues { get; private set; }
	internal static List<SpawnPoint> SpawnPoints { get; private set; }

	public static void Cleanup()
	{
		Game.ResetMap( System.Array.Empty<Entity>() );
		Decal.Clear( true, true );
	}

	private static int _index = 0;
	internal static void SpawnClue()
	{
		if ( Clues.IsNullOrEmpty() )
			return;

		if ( _index >= Clues.Count )
			_index = 0;

		Clues[_index++].Show();
	}

	[Event.Entity.PostSpawn]
	private static void PostLevelLoaded()
	{
		Clues = Entity.All.OfType<Clue>().ToList();
		SpawnPoints = Entity.All.OfType<SpawnPoint>().ToList();
	}
}
