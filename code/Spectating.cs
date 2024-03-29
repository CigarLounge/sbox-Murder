using Sandbox;

namespace Murder;

public static class Spectating
{
	[ConVar.ClientData] private static bool forced_spectator { get; set; }

	/// <summary>
	/// Whether or not the local player is force spectating.
	/// </summary>	
	public static bool IsForced
	{
		get => forced_spectator;
		internal set
		{
			forced_spectator = value;

			if ( !value || Game.LocalPawn is Player player && !player.IsAlive() )
				return;

			GameManager.Kill();
		}
	}

	/// <summary>
	/// The player we're currently spectating.
	/// </summary>
	public static Player Player { get; set; }

	private static int _spectatedPlayerIndex;

	/// <summary>
	/// Cycles through player list to find a spectating target.
	/// </summary>
	/// <param name="forward">Determines if we cycle forwards or backwards</param>
	public static void FindPlayer( bool forward )
	{
		var alivePlayers = Utils.GetPlayersWhere( p => p.IsAlive() );

		if ( alivePlayers.IsNullOrEmpty() )
			return;

		_spectatedPlayerIndex += forward ? 1 : -1;

		if ( _spectatedPlayerIndex >= alivePlayers.Count )
			_spectatedPlayerIndex = 0;
		else if ( _spectatedPlayerIndex < 0 )
			_spectatedPlayerIndex = alivePlayers.Count - 1;

		Player = alivePlayers[_spectatedPlayerIndex];
	}

	private static Carriable _carriable;

	[Event.Tick.Client]
	private static void SpectatingViewModels()
	{
		if ( !Player.IsValid() || !Player.IsFirstPersonMode )
		{
			_carriable?.DestroyViewModel();
			_carriable = null;

			return;
		}

		if ( _carriable != Player.ActiveCarriable )
		{
			_carriable?.DestroyViewModel();
			_carriable = Player.ActiveCarriable;
			_carriable?.CreateViewModel();
		}
	}
}
