using Sandbox;
using Sandbox.Effects;

namespace Murder;

public class FirstPersonCamera : CameraMode
{
	private readonly ScreenEffects _screenEffects = Camera.Main.FindOrCreateHook<ScreenEffects>();

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
			Camera.Main.RemoveHook( _screenEffects );
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
			_screenEffects.Saturation = _screenEffects.Saturation.LerpTo( 1f, 0.02f );
			_screenEffects.Vignette.Intensity = _screenEffects.Vignette.Intensity.LerpTo( 0f, 0.05f );
			_screenEffects.FilmGrain.Intensity = _screenEffects.FilmGrain.Intensity.LerpTo( 0f, 0.05f ); ;
		}
		else
		{
			_screenEffects.Saturation = _screenEffects.Saturation.LerpTo( 0f, 0.02f );
			_screenEffects.Vignette.Color = Color.Black;
			_screenEffects.Vignette.Roundness = 1f;
			_screenEffects.Vignette.Smoothness = 1f;
			_screenEffects.Vignette.Intensity = _screenEffects.Vignette.Intensity.LerpTo( 1.5f, 0.05f );
			_screenEffects.FilmGrain.Intensity = _screenEffects.FilmGrain.Intensity.LerpTo( 0.3f, 0.05f );
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
