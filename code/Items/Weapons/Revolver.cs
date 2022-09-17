using Sandbox;
using SandboxEditor;

namespace Murder;

[Category( "Weapons" )]
[ClassName( "murder_weapon_revolver" )]
[EditorModel( "models/weapons/w_mr96.vmdl" )]
[HammerEntity]
[Title( "Revolver" )]
public class Revolver : Weapon
{
	public override float Damage => 200;
	public override string ViewModelPath { get; } = "models/weapons/v_mr96.vmdl";
	public override string WorldModelPath { get; } = "models/weapons/w_mr96.vmdl";

	public override void SimulateAnimator( PawnAnimator animator )
	{
		base.SimulateAnimator( animator );

		animator.SetAnimParameter( "holdtype", 1 );
	}

	public override bool CanCarry( Player carrier )
	{
		return carrier.Role == Role.Bystander && base.CanCarry( carrier );
	}
}
