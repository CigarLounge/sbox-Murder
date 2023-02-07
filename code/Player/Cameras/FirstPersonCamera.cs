using Sandbox;
using Sandbox.Effects;

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
		if ( Game.LocalPawn is Player player && (player.IsAlive() || player.temp) )
		{
			player.temp = false;
			return;
		}

		if ( !Spectating.Player.IsValid() || Input.Pressed( InputButton.Jump ) )
		{
			Camera.Main.RemoveAllHooks<ScreenEffects>();
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
		var player = UI.Hud.DisplayedPlayer;

		if ( player.TimeUntilClean )
		{
			var postProcess = Camera.Main.FindOrCreateHook<ScreenEffects>();

			postProcess.Saturation = postProcess.Saturation.LerpTo( 1f, 0.02f );
			postProcess.Vignette.Intensity = postProcess.Vignette.Intensity.LerpTo( 0f, 0.05f );
			postProcess.FilmGrain.Intensity = postProcess.FilmGrain.Intensity.LerpTo( 0f, 0.05f ); ;
		}
		else
		{
			var postProcess = Camera.Main.FindOrCreateHook<ScreenEffects>();

			postProcess.Saturation = postProcess.Saturation.LerpTo( 0f, 0.02f );
			postProcess.Vignette.Color = Color.Black;
			postProcess.Vignette.Roundness = 1f;
			postProcess.Vignette.Smoothness = 1f;
			postProcess.Vignette.Intensity = postProcess.Vignette.Intensity.LerpTo( 1.5f, 0.05f );
			postProcess.FilmGrain.Intensity = postProcess.FilmGrain.Intensity.LerpTo( 0.3f, 0.05f );
		}

		Camera.Position = player.EyePosition;
		Camera.Rotation = !player.IsLocalPawn ? Rotation.Slerp( Camera.Rotation, player.EyeRotation, Time.Delta * 20f ) : player.EyeRotation;
		Camera.FirstPersonViewer = player;
	}

	[GameEvent.Player.Killed]
	private static void OnPlayerKilled( Player player )
	{
		if ( player == Spectating.Player )
			Current = new FreeCamera();
	}
}
