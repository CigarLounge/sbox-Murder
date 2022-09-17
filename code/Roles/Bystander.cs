namespace Murder;

[Category( "Roles" )]
[ClassName( "murder_role_bystander" )]
[Title( "Bystander" )]
public class Bystander : Role
{
	public override byte Ident => 1;
	public override Color Color { get; } = Color.Blue;
	public override string Title { get; } = "Bystander";
}
