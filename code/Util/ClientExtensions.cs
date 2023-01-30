using Sandbox;

namespace Murder;

public static class ClientExtensions
{
	public static bool HasRockedTheVote( this IClient client )
	{
		return client.GetValue<bool>( "!rtv" );
	}
}
