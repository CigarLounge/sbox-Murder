using Sandbox;
using System.Text.Json.Serialization;

namespace Murder;

public abstract class ItemInfo : GameResource
{
	[Title( "Icon" ), Category( "UI" ), ResourceType( "png" )]
	public string IconPath { get; set; } = "";

	[Category( "UI" )]
	public string Description { get; set; } = "";

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
