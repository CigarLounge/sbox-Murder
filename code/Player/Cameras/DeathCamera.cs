using Sandbox;

namespace Murder;

public class DeathCamera : CameraMode
{
	private Vector3 _focusPoint = Camera.Position;

	public DeathCamera()
	{
		Camera.Main.RemoveAllHooks<Sandbox.Effects.ScreenEffects>();
		Camera.FirstPersonViewer = null;

		BlackOut();
	}

	public override void BuildInput()
	{
		if ( Game.LocalPawn is not Player player )
			return;

		if ( player.TimeSinceDeath > 5f && Input.Pressed( InputButton.Jump ) )
			Current = new FreeCamera();
	}

	public override void FrameSimulate( IClient client )
	{
		if ( client.Pawn is not Player player || !player.Corpse.IsValid() )
			return;

		_focusPoint = Vector3.Lerp( _focusPoint, player.Corpse.WorldSpaceBounds.Center, Time.Delta * 5.0f );

		var tr = Trace.Ray( _focusPoint, _focusPoint + player.ViewAngles.ToRotation().Backward * 80 )
			.WorldOnly()
			.Run();

		Camera.Rotation = player.ViewAngles.ToRotation();
		Camera.Position = tr.EndPosition;
	}

	private async void BlackOut()
	{
		Camera.Main.RenderTags.Add( "corpse" );
		Camera.Main.ExcludeTags.Add( "world" );
		Camera.Main.ExcludeTags.Add( "light" );

		await GameTask.Delay( 5000 );

		Camera.Main.RenderTags.Remove( "corpse" );
		Camera.Main.ExcludeTags.RemoveAll();
	}
}
