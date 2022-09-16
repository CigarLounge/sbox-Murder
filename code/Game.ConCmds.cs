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

	[ConCmd.Admin( Name = "murder_giveitem" )]
	public static void GiveItem( string itemName )
	{
		if ( itemName.IsNullOrEmpty() )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var itemInfo = GameResource.GetInfo<ItemInfo>( itemName );
		if ( itemInfo is null )
		{
			Log.Error( $"{itemName} isn't a valid Item!" );
			return;
		}

		if ( itemInfo is CarriableInfo )
			player.Inventory.Add( TypeLibrary.Create<Carriable>( itemInfo.ClassName ) );
	}

	[ConCmd.Admin( Name = "murder_setrole" )]
	public static void SetRole( string roleName )
	{
		if ( Game.Current.State is not InProgress )
			return;

		var player = ConsoleSystem.Caller.Pawn as Player;
		if ( !player.IsValid() )
			return;

		var roleInfo = GameResource.GetInfo<RoleInfo>( roleName );
		if ( roleInfo is null )
		{
			Log.Error( $"{roleName} isn't a valid Role!" );
			return;
		}

		player.SetRole( roleInfo );
	}

	[ConCmd.Admin( Name = "murder_force_restart" )]
	public static void ForceRestart()
	{
		Game.Current.ChangeState( new PreRound() );
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
		Game.Current.RTVCount += 1;

		UI.ChatBox.AddInfo( To.Everyone, $"{client.Name} has rocked the vote! ({Game.Current.RTVCount}/{MathF.Round( Client.All.Count * Game.RTVThreshold )})" );
	}
}
