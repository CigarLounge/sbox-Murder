using Sandbox;

namespace Murder;

internal class DeathEffect
{
	[GameEvent.Player.Killed]
	private static async void OnPlayerKilled( Player player )
	{
		if ( !player.IsLocalPawn )
			return;

		Camera.Main.RenderTags.Add( "corpse" );
		Camera.Main.ExcludeTags.Add( "world" );
		Camera.Main.ExcludeTags.Add( "light" );

		await GameTask.Delay( 5000 );

		Camera.Main.RenderTags.Remove( "corpse" );
		Camera.Main.ExcludeTags.RemoveAll();

	}
}
