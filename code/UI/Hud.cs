using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Hud : RootPanel
{
	public Hud()
	{
		Local.Hud = this;
	}
}
