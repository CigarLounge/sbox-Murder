using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class NameHealthWrapper : Panel
{
	private NameHealthDisplay NameDisplay { get; set; }
	private bool _isShowing = false;

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		_isShowing = player.IsSpectatingPlayer || (player.Role != Role.None && player.IsAlive());
		if ( _isShowing )
		{
			NameDisplay ??= AddChild<NameHealthDisplay>();
		}
		else
		{
			NameDisplay?.Delete();
			NameDisplay = null;
		}
	}
}
