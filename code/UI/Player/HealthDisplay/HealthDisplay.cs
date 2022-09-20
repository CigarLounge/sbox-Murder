using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class HealthDisplay : Panel
{
	private NameDisplay NameDisplay { get; set; }
	private Label CluesCollected { get; init; }

	private bool _isShowing;

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		_isShowing = (player.Role != Role.None && player.IsAlive()) || player.IsSpectatingPlayer;
		if ( _isShowing )
		{
			CluesCollected.Text = $"{player.CluesCollected}";
			NameDisplay ??= AddChild<NameDisplay>();
		}
		else
		{
			CluesCollected.Text = string.Empty;

			NameDisplay?.Delete();
			NameDisplay = null;
		}
	}

	public override void DrawBackground( ref RenderState state )
	{
		if ( Local.Pawn is not Player player || !_isShowing )
			return;

		base.DrawBackground( ref state );

		var center = Box.Rect.Center;
		var draw = Render.Draw2D;

		// TODO: Wait for this fix https://github.com/Facepunch/sbox-issues/issues/2335
		draw.Material = Material.UI.Basic;

		var outlineRadius = Box.Rect.Width * 0.5f;
		draw.Color = Color.Black.WithAlpha( 0.9f );
		draw.Circle( center, outlineRadius );

		var radius = Box.Rect.Width * .4f;
		var healthPercentage = player.CurrentPlayer.Health / Player.MaxHealth;
		draw.Color = player.AssignedColour;
		draw.Circle( center, radius * healthPercentage );
	}
}