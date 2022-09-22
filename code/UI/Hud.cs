using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

public class Hud : RootPanel
{
	public Hud()
	{
		Local.Hud = this;

		StyleSheet.Load( "/UI/Hud.scss" );
		AddClass( "panel" );
		AddClass( "fullscreen" );

		Init();
	}

	private void Init()
	{
		AddChild<HintDisplay>();
		AddChild<ChatBox>();
		AddChild<VoiceChat>();
		AddChild<Scoreboard>();
		AddChild<NameHealthWrapper>();
		AddChild<RoleDisplay>();
		AddChild<InventoryWrapper>();
		AddChild<FullScreenHintMenu>();
	}

	[Event.Hotload]
	private void OnHotReload()
	{
		DeleteChildren( true );
		Init();
	}
}
