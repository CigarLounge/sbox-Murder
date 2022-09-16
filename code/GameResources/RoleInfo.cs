using Sandbox;
using System.Text.Json.Serialization;

namespace Murder;

[GameResource( "Role", "role", "Murder role template.", Icon = "🎭" )]
public class RoleInfo : GameResource
{
	[Category( "UI" )]
	public Color Color { get; set; }

	[Title( "Icon" ), Category( "UI" ), ResourceType( "png" )]
	public string IconPath { get; set; } = "ui/none.png";

	[HideInEditor]
	[JsonIgnore]
	public Texture Icon { get; private set; }

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( Host.IsClient )
			Icon = Texture.Load( FileSystem.Mounted, IconPath );
	}
}
