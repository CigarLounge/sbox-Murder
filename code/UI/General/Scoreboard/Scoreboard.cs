using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

namespace Murder.UI;

[UseTemplate]
public class Scoreboard : Panel
{
	private readonly Dictionary<Client, ScoreboardEntry> _entries = new();
	private readonly ScoreboardGroup[] _scoreboardGroups = new ScoreboardGroup[2];

	private Panel Content { get; init; }

	public Scoreboard()
	{
		_scoreboardGroups[0] = new ScoreboardGroup( Content, false );
		_scoreboardGroups[1] = new ScoreboardGroup( Content, true );
	}

	public void AddClient( Client client )
	{
		var scoreboardGroup = GetScoreboardGroup( client );
		var scoreboardEntry = scoreboardGroup.AddEntry( client );

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
		if ( scoreboardGroup != panel.Parent.Parent )
		{
			RemoveClient( client );
			AddClient( client );
		}
		else
		{
			panel.Update();
		}

		foreach ( var group in _scoreboardGroups )
			group.Enabled( group.GroupMembers != 0 );
	}

	private void RemoveClient( Client client )
	{
		if ( !_entries.TryGetValue( client, out var panel ) )
			return;

		var scoreboardGroup = (ScoreboardGroup)panel.Parent.Parent;

		if ( scoreboardGroup is not null )
			scoreboardGroup.GroupMembers--;

		panel.Delete();
		_entries.Remove( client );
	}

	public override void Tick()
	{
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

	private ScoreboardGroup GetScoreboardGroup( Client client )
	{
		var player = client.Pawn as Player;

		return _scoreboardGroups[player.IsForcedSpectator ? 1 : 0];
	}

	[Event.BuildInput]
	private void MenuInput( InputBuilder input )
	{
		this.Enabled( input.Down( InputButton.Score ) );
	}
}
