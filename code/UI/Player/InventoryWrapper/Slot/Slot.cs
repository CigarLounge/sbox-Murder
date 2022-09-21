using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Slot : Panel
{
	private Label Name { get; init; }
	private InputGlyph Glyph { get; init; }

	public Slot( string name, InputButton input )
	{
		Name.Text = name;
		Glyph.SetButton( input );
	}
}
