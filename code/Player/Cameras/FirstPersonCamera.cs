using Sandbox;

namespace Murder;

public class FirstPersonCamera : CameraMode
{
	public FirstPersonCamera( Player viewer = null )
	{
		Spectating.Player = viewer;
		Camera.FirstPersonViewer = viewer ?? Game.LocalPawn;
	}

	public override void BuildInput()
	{
		if ( Game.LocalPawn is Player player && (player.IsAlive() || !player.temp) )
			return;

		if ( !Spectating.Player.IsValid() || Input.Pressed( InputButton.Jump ) )
		{
			Current = new FreeCamera();
			return;
		}

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
			Spectating.FindPlayer( false );

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
			Spectating.FindPlayer( true );
	}

	public override void FrameSimulate( IClient client )
	{
		var target = UI.Hud.DisplayedPlayer;

		Camera.Position = target.EyePosition;
		Camera.Rotation = !target.IsLocalPawn ? Rotation.Slerp( Camera.Rotation, target.EyeRotation, Time.Delta * 20f ) : target.EyeRotation;
		Camera.FirstPersonViewer = target;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );
		Camera.Main.SetViewModelCamera( 95f );
	}
}
