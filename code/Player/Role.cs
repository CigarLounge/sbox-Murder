using System.Collections.Generic;

namespace Murder;

public enum Role
{
	None,
	Bystander,
	Murderer
}

public static class RoleExtensions
{
	public static Color GetColor( this Role role )
	{
		return role switch
		{
			Role.None => Color.Transparent,
			Role.Bystander => new Color32( 85, 212, 255 ),
			Role.Murderer => new Color32( 255, 56, 56 ),
			_ => Color.Transparent
		};
	}

	public static string GetTitle( this Role role )
	{
		return role switch
		{
			Role.None => "",
			Role.Bystander => "Bystander",
			Role.Murderer => "Murderer",
			_ => string.Empty
		};
	}
}
