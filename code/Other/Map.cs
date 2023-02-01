using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

// TODO: MOVE THIS SHIT TO GAMEMANAGER
internal static class Map
{
	internal static List<Clue> Clues { get; private set; }
	internal static List<SpawnPoint> SpawnPoints { get; private set; }

	public static void Cleanup()
	{
		Game.ResetMap( System.Array.Empty<Entity>() );
		Decal.Clear( true, true );
	}

	internal static void SpawnClue()
	{
		Game.Random.FromList( Clues ).Show();
	}

	[Event.Entity.PostSpawn]
	private static void PostLevelLoaded()
	{
		Clues = Entity.All.OfType<Clue>().ToList();
		SpawnPoints = Entity.All.OfType<SpawnPoint>().ToList();
	}
}
