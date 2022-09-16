using Sandbox;
using Sandbox.UI;

namespace Murder.UI;

[UseTemplate]
public class Nameplate : EntityHintPanel
{
	public readonly Player _player;

	private Label Name { get; init; }
	private Label HealthStatus { get; init; }
	private Label Role { get; init; }
	private Label Tag { get; init; }

	public Nameplate( Player player ) => _player = player;

	public override void Tick()
	{
		if ( !_player.IsValid() )
			return;

		var health = _player.Health / Player.MaxHealth * 100;
		var healthGroup = _player.GetHealthGroup( health );

		HealthStatus.Style.FontColor = healthGroup.Color;
		HealthStatus.Text = healthGroup.Title;

		Name.Text = _player.Client?.Name ?? "";

		Tag.Text = _player.TagGroup.Title;
		Tag.Style.FontColor = _player.TagGroup.Color;
	}
}
