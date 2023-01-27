using Sandbox;

namespace Murder;

public sealed class Disguise : EntityComponent<Player>
{
	// Murderer's original information.
	private string _bystanderName;
	private Color _color;
	private int _cluesCollected;

	private Player _murderer;

	public Disguise()
	{
		ShouldTransmit = false;
	}

	public override bool CanAddToEntity( Entity entity )
	{
		if ( entity is not Player player )
			return false;

		return player.Role == Role.Murderer;
	}

	public void SetPlayer( Player player )
	{
		Enabled = true;

		_murderer.BystanderName = player.BystanderName;
		_murderer.Color = player.Color;
		_murderer.CluesCollected = player.CluesCollected;
		_murderer.ColoredClothing.RenderColor = player.Color;
	}

	protected override void OnActivate()
	{
		if ( _murderer is not null )
			return;

		_murderer = Entity;

		_bystanderName = _murderer.BystanderName;
		_color = _murderer.Color;
		_cluesCollected = _murderer.CluesCollected;
	}

	protected override void OnDeactivate()
	{
		_murderer.BystanderName = _bystanderName;
		_murderer.Color = _color;
		_murderer.CluesCollected = _cluesCollected;
		_murderer.ColoredClothing.RenderColor = _color;
	}

	[GameEvent.Player.Killed]
	private static void OnPlayerKilled( Player player )
	{
		if ( !Game.IsServer )
			return;

		if ( player.LastAttacker is not Player attacker )
			return;

		if ( !attacker.Components.TryGet<Disguise>( out var disguise ) )
			return;

		disguise.Enabled = false;
	}

	[GameEvent.Round.End]
	private static void RemoveDisguise( Role winningRole )
	{
		if ( !Game.IsServer )
			return;

		foreach ( var murderer in Role.Murderer.GetPlayers() )
			murderer.Components.RemoveAny<Disguise>();
	}
}
