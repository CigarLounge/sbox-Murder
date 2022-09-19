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

		if ( player.IsSpectatingPlayer || player.IsAlive() )
		{
			NameDisplay ??= AddChild<NameDisplay>();
		}
		else
		{
			NameDisplay?.Delete();
			NameDisplay = null;
		}

		CluesCollected.Text = $"{player.CluesCollected}";
	}

	// TODO: Wait for this fix https://github.com/Facepunch/sbox-issues/issues/2335
	private float BorderWidth { get; set; } = 5;
	private float EdgeGap { get; set; } = 2;
	private int Points { get; set; } = 64;
	private float FillStart { get; set; } = .5f;
	private float FillAmount { get; set; } = 0.37f;
	private Color TrackColor { get; set; } = Color.White;
	private Color FillColor { get; set; } = Color.Blue;

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var center = Box.Rect.Center;
		var radius = Box.Rect.Width * .5f;
		var draw = Render.Draw2D;

		draw.Color = TrackColor;
		draw.CircleEx( center, radius, radius - BorderWidth, Points );

		draw.Color = FillColor;
		draw.CircleEx( center, radius - EdgeGap, radius - BorderWidth + EdgeGap, Points, FillStart * 360, (FillStart + FillAmount) * 360 );
	}
}