using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

public partial class Player
{
	public AnimatedEntity ColoredClothing { get; private set; }

	private readonly ClothingContainer _clothingContainer = new();
	private readonly List<Clothing> _playerClothing = new();

	public void DressPlayer()
	{
		_clothingContainer.Clothing = _playerClothing;
		_clothingContainer.DressEntity( this );

		ColoredClothing = (AnimatedEntity)Children.FirstOrDefault( x => x is AnimatedEntity m && m.Model.ResourcePath == "models/longsleeve/longsleeve.vmdl" );
	}

	private void SetupPlayerClothing()
	{
		_playerClothing.Add( ResourceLibrary.Get<Clothing>( "models/longsleeve/longsleeve.clothing" ) );

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

			if ( clothing.Category != Clothing.ClothingCategory.Tops )
				_playerClothing.Add( clothing );
		}
	}
}
