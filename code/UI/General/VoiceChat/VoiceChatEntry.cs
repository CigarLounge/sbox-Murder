using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Murder.UI;

[UseTemplate]
public class VoiceChatEntry : Panel
{
	public Friend Friend;

	private Label Name { get; init; }
	private Image Avatar { get; init; }


	private readonly Client _client;
	private float _targetVoiceLevel = 0;
	private float _voiceLevel = 0.5f;
	private readonly float _voiceTimeout = 0.1f;
	private readonly WorldPanel _worldPanel;

	RealTimeSince _timeSincePlayed;

	public VoiceChatEntry( Panel parent, Client client ) : base( parent )
	{
		_client = client;
		Friend = new( client.PlayerId );

		var player = (Player)client.Pawn;

		if ( player.IsAlive() )
		{
			Avatar.Style.BackgroundColor = player.Color;
			Name.Text = player.BystanderName;
		}
		else
		{
			Avatar.SetTexture( $"avatar:{client.PlayerId}" );
			Name.Text = Friend.Name;
		}

		_worldPanel = new WorldPanel();
		_worldPanel.StyleSheet.Load( "/UI/General/VoiceChat/VoiceChatEntry.scss" );
		_worldPanel.Add.Image( classname: "voice-icon" ).SetTexture( "ui/voicechat.png" );
		_worldPanel.SceneObject.Flags.ViewModelLayer = true;
		_worldPanel.Enabled( !_client.Pawn.IsFirstPersonMode );
	}

	public void Update( float level )
	{
		_timeSincePlayed = 0;
		_targetVoiceLevel = level;

		if ( _client.IsValid() )
			SetClass( "dead", !_client.Pawn.IsAlive() );
	}

	public override void Tick()
	{
		if ( IsDeleting )
			return;

		var timeoutInv = 1 - (_timeSincePlayed / _voiceTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			_worldPanel?.Delete();
			Delete();
			return;
		}

		_voiceLevel = _voiceLevel.LerpTo( _targetVoiceLevel, Time.Delta * 40.0f );

		if ( !_worldPanel.IsValid() || _client.Pawn is not Player player || !player.IsAlive() )
		{
			_worldPanel?.Delete();
			return;
		}

		if ( !_worldPanel.IsEnabled() )
			return;

		var tx = player.GetBoneTransform( "head" );
		tx.Position += Vector3.Up * (Vector3.Up * _voiceLevel);
		tx.Rotation = CurrentView.Rotation.RotateAroundAxis( Vector3.Up, 180f );
		_worldPanel.Transform = tx;
	}
}
