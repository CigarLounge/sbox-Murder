namespace Murder;

[Category( "Roles" )]
[ClassName( "murder_role_none" )]
[Title( "None" )]
public class NoneRole : Role
{
	public override byte Ident => 0;
	public override Color Color { get; } = Color.White;
	public override string Title { get; } = "None";
}
