using Sandbox;
using System.Collections.Generic;

namespace Murder;

public partial class Player
{
	public static List<Model> ColorableModels { get; private set; } = new List<Model>() { Model.Load( "models/longsleeve/longsleeve.vmdl" ) };
	public ClothingContainer ClothingContainer { get; private init; } = new();
	private List<AnimatedEntity> ColoredClothing { get; init; } = new();

	private void SetupClothing()
	{
		ClothingContainer.LoadFromClient( Client );

		ClothingContainer.Toggle( ResourceLibrary.Get<Clothing>( "models/longsleeve/longsleeve.clothing" ) );
		ClothingContainer.Toggle( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/jeans/jeans_black.clothing" ) );
		ClothingContainer.Toggle( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/trainers/trainers.clothing" ) );
	}
}
