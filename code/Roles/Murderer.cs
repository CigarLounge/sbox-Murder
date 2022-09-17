namespace Murder;

[Category( "Roles" )]
[ClassName( "murder_role_murderer" )]
[Title( "Murderer" )]
public class Murderer : Role
{
	public override byte Ident => 2;
	public override Color Color { get; } = Color.Blue;
	public override string Title { get; } = "Bystander";
}
