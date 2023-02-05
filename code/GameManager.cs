using Sandbox;
using Sandbox.Diagnostics;
using System.Collections.Generic;

namespace Murder;

public partial class GameManager : Sandbox.GameManager
{
	public static GameManager Instance { get; private set; }

	[Net, Change]
	public GameState State { get; private set; }
	private GameState _lastState;

	[Net]
	public IList<string> MapVoteIdents { get; set; }

	[Net]
	public int TotalRoundsPlayed { get; set; }

	public int RTVCount { get; set; }

	public GameManager()
	{
		Instance = this;

		LoadResources();

		if ( Game.IsClient )
		{
			_ = new UI.Hud();

			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
			Camera.Main.SetViewModelCamera( 95f );
		}
	}

	public override void FrameSimulate( IClient client )
	{
		if ( client.Pawn is not Entity entity || !entity.IsValid() || !entity.IsAuthority )
			return;

		entity.FrameSimulate( client );
		CameraMode.Current.FrameSimulate( client );
	}

	public override void BuildInput()
	{
		CameraMode.Current.BuildInput();

		base.BuildInput();
	}

	/// <summary>
	/// Changes the state if minimum players is met. Otherwise, force changes to "WaitingState"
	/// </summary>
	/// <param name="state"> The state to change to if minimum players is met.</param>
	public void ChangeState( GameState state )
	{
		Game.AssertServer();
		Assert.NotNull( state );

		ForceStateChange( HasMinimumPlayers() ? state : new WaitingState() );
	}

	/// <summary>
	/// Force changes a state regardless of player count.
	/// </summary>
	/// <param name="state"> The state to change to.</param>
	public void ForceStateChange( GameState state )
	{
		Game.AssertServer();

		State?.Finish();
		State = state;
		State.Start();
	}

	public override void OnKilled( Entity pawn )
	{
		// Do nothing. Base implementation just adds to a kill feed and prints to console.
	}

	public override void ClientJoined( IClient client )
	{
		var player = new Player( client );

		State.OnPlayerJoin( player );

		UI.TextChat.AddInfo( $"{client.Name} has joined" );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		State.OnPlayerLeave( client.Pawn as Player );

		UI.TextChat.AddInfo( $"{client.Name} has left ({reason})" );

		var player = (Player)client.Pawn;

		// Only delete the pawn if they are alive.
		// Keep the dead body otherwise on disconnect.
		if ( player.IsValid() && player.IsAlive() )
			client.Pawn.Delete();

		player.Tags.Remove( "ignorereset" );
	}

	public override bool CanHearPlayerVoice( IClient source, IClient dest )
	{
		if ( source.Pawn is not Player sourcePlayer || dest.Pawn is not Player destPlayer )
			return false;

		if ( destPlayer.MuteFilter == MuteFilter.All )
			return false;

		if ( !sourcePlayer.IsAlive() && !destPlayer.CanHearSpectators )
			return false;

		if ( sourcePlayer.IsAlive() && !destPlayer.CanHearAlivePlayers )
			return false;

		return true;
	}

	public override void OnVoicePlayed( IClient client )
	{
		UI.VoiceChat.OnVoicePlayed( client );
	}

	public override void PostLevelLoaded()
	{
		ForceStateChange( new WaitingState() );
	}

	[Event.Tick]
	private void Tick()
	{
		State?.OnTick();
	}

	public static bool HasMinimumPlayers()
	{
		return Utils.GetPlayersWhere( p => !p.IsForcedSpectator ).Count >= MinPlayers;
	}

	private static void LoadResources()
	{
		Player.ClothingPreset.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/hair/eyebrows/eyebrows_black.clothing" ) );
		Player.ClothingPreset.Add( ResourceLibrary.Get<Clothing>( "models/longsleeve/longsleeve.clothing" ) );
		Player.ClothingPreset.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/jeans/jeans_black.clothing" ) );
		Player.ClothingPreset.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/trainers/trainers.clothing" ) );

		Player.Footprint = ResourceLibrary.Get<DecalDefinition>( "decals/footprint.decal" );
	}

	private void OnStateChanged( GameState oldState, GameState newState )
	{
		_lastState?.Finish();
		_lastState = newState;
		_lastState?.Start();
	}
}
