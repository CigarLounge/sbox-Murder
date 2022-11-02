using Sandbox;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_holster" )]
[Title( "Holster" )]
public partial class Holster : Carriable
{
	public override string Title { get; } = "Holster";
	public override SlotType Slot { get; } = SlotType.Holster;
	public override string ViewModelPath { get; } = "";
	public override string WorldModelPath { get; } = "";

	public override void SimulateAnimator( PawnAnimator animator )
	{
		base.SimulateAnimator( animator );

		animator.SetAnimParameter( "holdtype", 0 );
	}
}
