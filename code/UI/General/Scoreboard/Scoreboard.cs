using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Murder.UI;

[UseTemplate]
public class Scoreboard : Panel
{
	private readonly Dictionary<Client, ScoreboardEntry> _entries = new();
	private readonly Dictionary<LifeState, ScoreboardGroup> _scoreboardGroups = new();

	private Panel Content { get; init; }

	public Scoreboard()
	{
		AddScoreboardGroup( LifeState.Alive );
		AddScoreboardGroup( LifeState.Dead );
	}

	public void AddClient( Client client )
	{
		var scoreboardGroup = GetScoreboardGroup( client );
		var scoreboardEntry = scoreboardGroup.AddEntry( client );

		if ( !client.Pawn.IsLocalPawn && client.Pawn.IsAlive() )
		{
			scoreboardEntry.AddEventListener( "onclick", () => scoreboardEntry.OnClick() );
			scoreboardEntry.Style.Cursor = "pointer";
		}

		scoreboardGroup.GroupMembers++;

		_entries.Add( client, scoreboardEntry );
	}

	private void UpdateClient( Client client )
	{
		if ( client is null )
			return;

		if ( !_entries.TryGetValue( client, out var panel ) )
			return;

		var scoreboardGroup = GetScoreboardGroup( client );
		if ( scoreboardGroup.GroupStatus != panel.PlayerStatus )
		{
			RemoveClient( client );
			AddClient( client );
		}
		else
		{
			panel.Update();
		}

		foreach ( var value in _scoreboardGroups.Values )
			value.Style.Display = value.GroupMembers == 0 ? DisplayMode.None : DisplayMode.Flex;
	}

	private void RemoveClient( Client client )
	{
		if ( !_entries.TryGetValue( client, out var panel ) )
			return;

		_scoreboardGroups.TryGetValue( panel.PlayerStatus, out var scoreboardGroup );

		if ( scoreboardGroup is not null )
			scoreboardGroup.GroupMembers--;

		panel.Delete();
		_entries.Remove( client );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible )
			return;

		foreach ( var client in Client.All.Except( _entries.Keys ) )
		{
			AddClient( client );
			UpdateClient( client );
		}

		foreach ( var client in _entries.Keys.Except( Client.All ) )
		{
			if ( _entries.TryGetValue( client, out var row ) )
			{
				row?.Delete();
				RemoveClient( client );
			}
		}

		foreach ( var client in Client.All )
			UpdateClient( client );
	}

	private ScoreboardGroup AddScoreboardGroup( LifeState someState )
	{
		var scoreboardGroup = new ScoreboardGroup( Content, someState );
		_scoreboardGroups.Add( someState, scoreboardGroup );
		return scoreboardGroup;
	}

	private ScoreboardGroup GetScoreboardGroup( Client client )
	{
		var player = client.Pawn as Player;
		return _scoreboardGroups[player.LifeState];
	}

	[Event.BuildInput]
	private void MenuInput( InputBuilder input )
	{
		this.Enabled( input.Down( InputButton.Score ) );
	}
}
