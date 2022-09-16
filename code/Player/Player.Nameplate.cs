namespace Murder;

public partial class Player : IEntityHint
{
	public float HintDistance => MaxHintDistance;

	UI.EntityHintPanel IEntityHint.DisplayHint( Player player )
	{
		return new UI.Nameplate( this );
	}
}
