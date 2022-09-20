using System;
using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class HealthDisplay : Panel
{
	private NameDisplay NameDisplay { get; set; }
	private Label CluesCollected { get; init; }

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		var isEnabled = player.IsSpectatingPlayer || player.IsAlive();
		if ( isEnabled )
		{
			NameDisplay ??= AddChild<NameDisplay>();
		}
		else
		{
			NameDisplay?.Delete();
			NameDisplay = null;
		}

		CluesCollected.Text = isEnabled ? $"{player.CluesCollected}" : string.Empty;
	}

	public override void DrawBackground( ref RenderState state )
	{
		if ( Local.Pawn is not Player player || !player.CurrentPlayer.IsAlive() )
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