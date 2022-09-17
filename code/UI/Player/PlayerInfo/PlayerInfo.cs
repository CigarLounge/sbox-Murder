using Sandbox;
using Sandbox.UI;
using System.Threading.Tasks;

namespace Murder.UI;

[UseTemplate]
public class PlayerInfo : Panel
{
	public static PlayerInfo Instance;

	private Panel RoleContainer { get; init; }
	private Label RoleTitle { get; init; }

	private Panel HealthContainer { get; init; }
	private Label Health { get; init; }

	public PlayerInfo()
	{
		Instance = this;
	}

	[GameEvent.Player.TookDamage]
	private void OnHit( Player player )
	{
		if ( !this.IsEnabled() )
			return;

		_ = TakeHit();
	}


	private async Task TakeHit()
	{
		AddClass( "hit" );
		await GameTask.Delay( 200 );
		RemoveClass( "hit" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as Player;

		this.Enabled( player.CurrentPlayer.IsValid() && player.CurrentPlayer.IsAlive() );
		if ( !this.IsEnabled() )
			return;

		if ( player.CurrentPlayer.Role == Role.None )
		{
			RoleContainer.Enabled( false );
			HealthContainer.Style.BorderTopLeftRadius = 4;
			HealthContainer.Style.BorderBottomLeftRadius = 4;
		}
		else
		{
			RoleContainer.Enabled( true );
			RoleContainer.Style.BackgroundColor = player.CurrentPlayer.Role.GetColor();
			RoleTitle.Text = player.CurrentPlayer.Role.GetTitle();

			HealthContainer.Style.BorderTopLeftRadius = 0;
			HealthContainer.Style.BorderBottomLeftRadius = 0;
		}

		Health.Text = $"✚ {player.CurrentPlayer.Health.CeilToInt()}";
	}
}
