using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

namespace Murder.UI;

public partial class MapSelect : Panel
{
	private Panel Maps { get; set; }

	public MapSelect()
	{
		// Delete unneeded UI elements.
		foreach ( var panel in Game.RootPanel.Children.ToList() )
		{
			if ( panel is not TextChat and not VoiceChat )
				panel.Delete( true );
		}
	}

	public override void Tick()
	{
		if ( GameState.Current is not MapSelectionState mapSelectionState )
			return;

		// We are looping quite a lot in this code. Maybe we can use razor to make this less painful?
		var maps = Maps.ChildrenOfType<MapIcon>();

		foreach ( var icon in maps )
			icon.Votes = 0;

		foreach ( var group in mapSelectionState.Votes.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ) )
		{
			foreach ( var map in maps )
			{
				if ( group.Key == map.Ident )
					map.Votes = group.Count();
			}
		}
	}

	protected override int BuildHash() => HashCode.Combine( GameState.Current.TimeLeftFormatted );
}
