using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class ScoreboardEntry : Panel
{
	private Image PlayerAvatar { get; init; }
	private Label PlayerName { get; init; }
	private Label Ping { get; init; }

	private readonly Client _client;

	public ScoreboardEntry( Panel parent, Client client ) : base( parent )
	{
		_client = client;
	}

	public void Update()
	{
		PlayerName.Text = _client.Name;
		Ping.Text = _client.IsBot ? "BOT" : _client.Ping.ToString();
		PlayerAvatar.SetTexture( $"avatar:{_client.PlayerId}" );
	}
}
