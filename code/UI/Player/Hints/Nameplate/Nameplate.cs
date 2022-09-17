using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Nameplate : EntityHintPanel
{
	public readonly Player _player;

	private Label Name { get; init; }

	public Nameplate( Player player ) => _player = player;

	public override void Tick()
	{
		if ( !_player.IsValid() )
			return;

		Name.Text = _player.AssignedName;
	}
}
