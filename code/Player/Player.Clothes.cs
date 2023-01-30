using Sandbox;
using System.Collections.Generic;
using System.Linq;

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
		ColoredClothing = (AnimatedEntity)Children.FirstOrDefault( x => x is AnimatedEntity m && m.Model.ResourcePath == "models/longsleeve/longsleeve.vmdl" );
	}
}
