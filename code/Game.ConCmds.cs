using Sandbox;
using System;

namespace Murder;

public partial class GameManager
{
	[ConCmd.Admin( Name = "mur_takedamage" )]
	public static void GiveItem( float damage )
	{
		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		player.TakeDamage( new DamageInfo { Damage = damage } );
	}

	[ConCmd.Admin( Name = "mur_giveclues" )]
	public static void GiveClues( int clues )
	{
		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		player.Clues += clues;
	}

	[ConCmd.Admin( Name = "mur_giveitem" )]
	public static void GiveItem( string itemName )
	{
		if ( string.IsNullOrEmpty( itemName ) )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		if ( itemName.Equals( "revolver", StringComparison.OrdinalIgnoreCase ) )
			player.SetCarriable( new Revolver() );
		else if ( itemName.Equals( "knife", StringComparison.OrdinalIgnoreCase ) )
			player.SetCarriable( new Knife() );
	}

	[ConCmd.Admin( Name = "mur_setrole" )]
	public static void SetRole( Role role )
	{
		if ( Instance.State is not GameplayState )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		player.Role = role;
	}

	[ConCmd.Admin( Name = "mur_force_restart" )]
	public static void ForceRestart()
	{
		Instance.ChangeState( new GameplayState() );
	}

	[ConCmd.Server( Name = "mur_rtv" )]
	public static void RockTheVote()
	{
		var client = ConsoleSystem.Caller;

		if ( !client.IsValid() )
			return;

		if ( client.GetValue<bool>( "rtv" ) )
			return;

		client.SetValue( "rtv", true );
		Instance.RTVCount += 1;

		UI.TextChat.AddInfo( $"{client.Name} has rocked the vote! ({Instance.RTVCount}/{MathF.Round( Game.Clients.Count * GameManager.RTVThreshold )})" );
	}

	[ConCmd.Server( Name = "kill" )]
	public static void Kill()
	{
		var player = ConsoleSystem.Caller.Pawn as Player;

		if ( !player.IsValid() )
			return;

		player.Kill();
	}
}
