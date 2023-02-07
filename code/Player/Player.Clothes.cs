using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

public partial class Player
{
	public AnimatedEntity ColoredClothing { get; private set; }
	public List<Clothing> Clothes { get; private set; } = new();

	private readonly ClothingContainer _clothingContainer = new();

	public void DressPlayerWith( List<Clothing> clothing )
	{
		_clothingContainer.Clothing = clothing;
		_clothingContainer.DressEntity( this );

		ColoredClothing = (AnimatedEntity)Children.FirstOrDefault( x => x is AnimatedEntity m && m.Model.ResourcePath == "models/longsleeve/longsleeve.vmdl" );
	}

	private void SetupPlayerClothes()
	{
		Clothes.Add( ResourceLibrary.Get<Clothing>( "models/longsleeve/longsleeve.clothing" ) );
		Clothes.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/trousers/jeans/jeans_black.clothing" ) );
		Clothes.Add( ResourceLibrary.Get<Clothing>( "models/citizen_clothes/shoes/trainers/trainers.clothing" ) );

		var data = Client.GetClientData( "avatar" );
		if ( string.IsNullOrEmpty( data ) )
			return;

		var entries = System.Text.Json.JsonSerializer.Deserialize<ClothingContainer.Entry[]>( data );
		if ( entries.IsNullOrEmpty() )
			return;

		foreach ( var entry in entries )
		{
			var clothing = ResourceLibrary.Get<Clothing>( entry.Id );
			if ( clothing is null )
				continue;

			if ( clothing.Category == Clothing.ClothingCategory.Hat ||
				 clothing.Category == Clothing.ClothingCategory.Hair ||
				 clothing.Category == Clothing.ClothingCategory.Facial ||
				 clothing.Category == Clothing.ClothingCategory.Skin )
				Clothes.Add( clothing );
		}
	}
}
