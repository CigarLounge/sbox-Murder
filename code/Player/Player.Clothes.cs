using Sandbox;
using System.Collections.Generic;

namespace Murder;

public partial class Player
{
	public static List<Clothing> ClothingPreset { get; private set; } = new();
	public ClothingContainer ClothingContainer { get; private init; } = new();

	public AnimatedEntity ColoredClothing { get; private set; }

	public void DressPlayer()
	{
		ClothingContainer.Clothing = ClothingPreset;
		ClothingContainer.DressEntity( this );

		// This is inconsistent.
		ColoredClothing = Children[^3] as AnimatedEntity;
	}
}
