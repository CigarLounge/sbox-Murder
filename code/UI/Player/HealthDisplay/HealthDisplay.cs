using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class HealthDisplay : Panel
{
	public Label Name { get; init; }
	public Label CluesCollected { get; init; }

	public float BorderWidth { get; set; } = 5;
	public float EdgeGap { get; set; } = 2;
	public int Points { get; set; } = 64;
	public float FillStart { get; set; } = .5f;
	public float FillAmount { get; set; } = 0.37f;
	public Color TrackColor { get; set; } = Color.White;
	public Color FillColor { get; set; } = Color.Blue;

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

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		Name.Text = player.AssignedName;
		Name.Style.FontColor = player.AssignedColour;

		CluesCollected.Text = $"{player.CluesCollected}";
	}
}