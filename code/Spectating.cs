namespace Murder;

public static class Spectating
{
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
}
