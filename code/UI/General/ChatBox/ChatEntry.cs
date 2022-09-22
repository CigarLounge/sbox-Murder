using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class ChatBoxEntry : Panel
{
	private Label Name { get; init; }
	private Label Message { get; init; }

	private RealTimeSince _timeSinceCreation;

	public ChatBoxEntry( string name, string message, Color? color = null )
	{
		_timeSinceCreation = 0;
		Name.Text = name;
		Message.Text = message;

		if ( color is not null )
			Name.Style.FontColor = color;
	}

	public override void Tick()
	{
		if ( _timeSinceCreation < 8 )
			return;

		AddClass( "faded" );
	}
}

