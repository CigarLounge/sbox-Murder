using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Murder;

public sealed partial class MapSelectionState : GameState
{
	[Net] public IDictionary<IClient, string> Votes { get; private set; }
	public override int Duration => GameManager.MapSelectionTime;
	public const string MapsFile = "maps.txt";

	protected override void OnTimeUp()
	{
		if ( Votes.Count == 0 )
		{
			Game.ChangeLevel( Game.Random.FromList( GameManager.Instance.MapVoteIdents.ToList() ) ?? GameManager.DefaultMap );
			return;
		}

		Game.ChangeLevel
		(
			Votes.GroupBy( x => x.Value )
			.OrderBy( x => x.Count() )
			.Last().Key
		);
	}

	protected override void OnStart()
	{
		Game.RootPanel?.AddChild<UI.MapSelect>();
	}

	[ConCmd.Server]
	public static void SetVote( string map )
	{
		if ( Current is not MapSelectionState state )
			return;

		if ( ConsoleSystem.Caller.Pawn is not Player player )
			return;

		state.Votes[player.Client] = map;
	}

	[Event.Entity.PostSpawn]
	private static async void OnFinishedLoading()
	{
		var maps = await GetLocalMapIdents();

		if ( maps.IsNullOrEmpty() )
			maps = await GetRemoteMapIdents();

		maps.Shuffle();
		GameManager.Instance.MapVoteIdents = maps;
	}

	private static async Task<List<string>> GetLocalMapIdents()
	{
		var maps = new List<string>();
		var rawMaps = FileSystem.Data.ReadAllText( MapsFile );

		if ( string.IsNullOrEmpty( rawMaps ) )
			return maps;

		var splitMaps = rawMaps.Split( "\n" );

		foreach ( var rawMap in splitMaps )
		{
			var mapIdent = rawMap.Trim();
			var package = await Package.Fetch( mapIdent, true );

			if ( package is not null && package.PackageType == Package.Type.Map )
				maps.Add( mapIdent );
			else
				Log.Error( $"{mapIdent} does not exist as a s&box map!" );
		}

		return maps;
	}

	private static async Task<List<string>> GetRemoteMapIdents()
	{
		var queryResult = await Package.FindAsync( "type:map game:matt.murder", take: 99 );

		return queryResult.Packages.Select( ( p ) => p.FullIdent ).ToList();
	}
}
