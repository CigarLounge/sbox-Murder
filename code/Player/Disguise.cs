using Sandbox;
using System.Collections.Generic;

namespace Murder;

public sealed class Disguise : EntityComponent<Player>
{
	// Murderer's original information.
	private int _nameIndex;
	private Color _color;
	private List<Clothing> _clothing;
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

		_murderer.ClothingContainer.Clothing = player.ClothingContainer.Clothing;
		_murderer.ClothingContainer.DressEntity( _murderer );

		_murderer.NameIndex = player.NameIndex;
		_murderer.Color = player.Color;
	}

	protected override void OnActivate()
	{
		if ( _murderer is not null )
			return;

		_murderer = Entity;
		_nameIndex = _murderer.NameIndex;
		_color = _murderer.Color;
		_clothing = _murderer.ClothingContainer.Clothing;
	}

	protected override void OnDeactivate()
	{
		_murderer.ClothingContainer.Clothing = _clothing;

		if ( _murderer.IsAlive() )
			_murderer.ClothingContainer.DressEntity( _murderer );

		_murderer.NameIndex = _nameIndex;
		_murderer.Color = _color;
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

		foreach ( var murderer in Utils.GetPlayersWhere( p => p.Role == Role.Murderer ) )
			murderer.Components.RemoveAny<Disguise>();
	}
}
