using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Nameplate : EntityHintPanel
{
	private static Nameplate _previous;
	private readonly Player _player;

	private Label Name { get; init; }

	public Nameplate( Player player )
	{
		_previous?.Delete( true );

		_player = player;
		_previous = this;
	}

	public override void Tick()
	{
		if ( !_player.IsValid() )
			return;

		Name.Text = _player.AssignedName;
		Name.Style.FontColor = _player.AssignedColour;
	}
}
