using Sandbox.UI;

namespace Murder;

public partial class Player : IEntityHint
{
	public float HintDistance => MaxHintDistance;

	Panel IEntityHint.DisplayHint( Player player )
	{
		return new UI.Nameplate( this );
	}
}
