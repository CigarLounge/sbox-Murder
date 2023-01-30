using Sandbox;

namespace Murder;

public enum MuteFilter
{
	None,
	AlivePlayers,
	Spectators,
	All
}

public partial class Player
{
	/// <summary>
	/// Determines which players are currently muted.
	/// </summary>
	[ConVar.ClientData( "mute_filter" )]
	public MuteFilter MuteFilter { get; set; } = MuteFilter.None;

	public bool CanHearSpectators => !IsIdentityHidden && MuteFilter != MuteFilter.Spectators && MuteFilter != MuteFilter.All;
	public bool CanHearAlivePlayers => MuteFilter != MuteFilter.AlivePlayers && MuteFilter != MuteFilter.All;

	public static void ToggleMute()
	{
		var player = Game.LocalPawn as Player;

		if ( ++player.MuteFilter > MuteFilter.All )
			player.MuteFilter = MuteFilter.None;
	}
}
