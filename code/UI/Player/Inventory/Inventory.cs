using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Inventory : Panel
{
	private Image Icon { get; init; }

	public override void Tick()
	{
		if ( Local.Pawn is not Player player )
			return;

		var carriable = player.CurrentPlayer.Carriable;

		this.Enabled( carriable.IsValid() );

		if ( !this.IsEnabled() )
			return;

		Icon.SetTexture( carriable.IconPath );
		Icon.SetClass( "active", carriable.IsActive );
	}
}
