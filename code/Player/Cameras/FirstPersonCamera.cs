using Sandbox;
using System;

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

		if ( target.TimeUntilClean )
		{
			var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

			postProcess.Saturation = postProcess.Saturation.LerpTo( 1f, 0.02f );
			postProcess.Vignette.Intensity = postProcess.Vignette.Intensity.LerpTo( 0f, 0.05f );
			postProcess.FilmGrain.Intensity = postProcess.FilmGrain.Intensity.LerpTo( 0f, 0.05f ); ;
		}
		else
		{
			var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();

			postProcess.Saturation = postProcess.Saturation.LerpTo( 0f, 0.02f );
			postProcess.Vignette.Color = Color.Black;
			postProcess.Vignette.Roundness = 1f;
			postProcess.Vignette.Smoothness = 1f;
			postProcess.Vignette.Intensity = postProcess.Vignette.Intensity.LerpTo( 1.5f, 0.05f );
			postProcess.FilmGrain.Intensity = postProcess.FilmGrain.Intensity.LerpTo( 0.3f, 0.05f );
		}

		Camera.Position = target.EyePosition;
		Camera.Rotation = !target.IsLocalPawn ? Rotation.Slerp( Camera.Rotation, target.EyeRotation, Time.Delta * 20f ) : target.EyeRotation;
		Camera.FirstPersonViewer = target;
	}
}
