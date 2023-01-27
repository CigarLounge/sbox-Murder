using Sandbox;

namespace Murder;

public partial class GameManager
{
	#region Round
	[ConVar.Server( "murder_postround_time", Help = "The length of the postround time.", Saved = true )]
	public static int PostRoundTime { get; set; } = 10;

	[ConVar.Server( "murder_mapselection_time", Help = "The length of the map selection period.", Saved = true )]
	public static int MapSelectionTime { get; set; } = 15;
	#endregion

	#region Debug
	[ConVar.Server( "murder_round_debug", Help = "Stop the in progress round from ending.", Saved = true )]
	public static bool PreventWin { get; set; }
	#endregion

	#region Map
	[ConVar.Server( "murder_default_map", Help = "The default map to swap to if no maps are found.", Saved = true )]
	public static string DefaultMap { get; set; } = "facepunch.flatgrass";

	[ConVar.Server( "murder_rtv_threshold", Help = "The percentage of players needed to RTV.", Saved = true )]
	public static float RTVThreshold { get; set; } = 0.66f;

	[ConVar.Replicated( "murder_round_limit", Help = "The maximum amount of rounds that can be played.", Saved = true )]
	public static int RoundLimit { get; set; } = 6;
	#endregion

	#region Minimum Players
	[ConVar.Replicated( "murder_min_players", Help = "The minimum players to start the game.", Saved = true )]
	public static int MinPlayers { get; set; } = 2;
	#endregion

	#region AFK Timers
	[ConVar.Server( "murder_afk_timer", Help = "The amount of time before a player is marked AFK.", Saved = true )]
	public static int AFKTimer { get; set; } = 180;

	[ConVar.Server( "murder_afk_kick", Help = "Kick any players that get marked AFK.", Saved = true )]
	public static bool KickAFKPlayers { get; set; }
	#endregion
}
