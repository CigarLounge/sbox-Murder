using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace Murder.UI;

[UseTemplate]
public class VoiceChat : Panel
{
	public static VoiceChat Instance { get; private set; }

	public VoiceChat() => Instance = this;

	public static void OnVoicePlayed( Client client )
	{
		var entry = Instance.ChildrenOfType<VoiceChatEntry>().FirstOrDefault( x => x.Friend.Id == client.PlayerId ) ?? new VoiceChatEntry( Instance, client );
		entry.Update( client.VoiceLevel );
	}

	public override void Tick()
	{
		if ( Voice.IsRecording )
			OnVoicePlayed( Local.Client );
	}
}
