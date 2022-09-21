using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class RadialHealth : Panel
{
	public Panel Radial { get; init; }
	public Label CluesCollected { get; init; }
}
