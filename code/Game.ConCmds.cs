using Sandbox;
using System;

namespace Murder;

public partial class Game
{
	[ConCmd.Admin( Name = "murder_respawn", Help = "Respawns the current player or the player with the given id" )]
	public static void RespawnPlayer( int id = 0 )
	{
		var player = id == 0 ? ConsoleSystem.Caller.Pawn as Player : Entity.FindByIndex( id ) as Player;
		if ( !player.IsValid() )
			return;

		if ( player.IsForcedSpectator )
			player.ToggleForcedSpectator();

		player.Respawn();
	}

	[ConCmd.Admin( Name = "murder_takedamage" )]
	public static void GiveItem( int damage )
	{
		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		player.TakeDamage( new DamageInfo { Damage = damage } );
	}

	[ConCmd.Admin( Name = "murder_giveitem" )]
	public static void GiveItem( string itemName )
	{
		if ( itemName.IsNullOrEmpty() )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		if ( itemName.Equals( "revolver", StringComparison.OrdinalIgnoreCase ) )
			player.SetCarriable( new Revolver() );
		else if ( itemName.Equals( "knife", StringComparison.OrdinalIgnoreCase ) )
			player.SetCarriable( new Knife() );
	}

	[ConCmd.Admin( Name = "murder_setrole" )]
	public static void SetRole( Role role )
	{
		if ( Current.State is not GameplayState )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		player.Role = role;
	}

	[ConCmd.Admin( Name = "murder_force_restart" )]
	public static void ForceRestart()
	{
		Game.Current.ChangeState( new GameplayState() );
	}

	[ConCmd.Server( Name = "murder_forcespec" )]
	public static void ToggleForceSpectator()
	{
		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		player.ToggleForcedSpectator();
	}

	[ConCmd.Server( Name = "murder_rtv" )]
	public static void RockTheVote()
	{
		var client = ConsoleSystem.Caller;
		if ( !client.IsValid() )
			return;

		if ( client.GetValue<bool>( Strings.HasRockedTheVote ) )
			return;

		client.SetValue( Strings.HasRockedTheVote, true );
		Current.RTVCount += 1;

		UI.TextChat.AddInfo( To.Everyone, $"{client.Name} has rocked the vote! ({Game.Current.RTVCount}/{MathF.Round( Client.All.Count * Game.RTVThreshold )})" );
	}
}
