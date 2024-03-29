using Sandbox.UI;

namespace Murder.UI;

public class HintDisplay : Panel
{
	public static HintDisplay Instance { get; set; }

	public HintDisplay()
	{
		Instance = this;
		AddClass( "fullscreen" );
	}
}

public partial class Hint : Panel
{
	public string HintText { get; set; }
}
