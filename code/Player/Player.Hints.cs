using Sandbox;
using Sandbox.UI;

namespace Murder;

public partial class Player
{
	public const float MaxHintDistance = 5000f;

	private static Panel _currentHintPanel;
	private static IEntityHint _currentHint;

	private void DisplayEntityHints()
	{
		if ( !UI.Hud.DisplayedPlayer.IsFirstPersonMode || !UI.Hud.DisplayedPlayer.TimeUntilClean )
		{
			DeleteHint();
			return;
		}

		HoveredEntity = FindHovered();

		if ( HoveredEntity is not IEntityHint hint || _traceDistance > hint.HintDistance || !hint.CanHint( UI.Hud.DisplayedPlayer ) )
		{
			DeleteHint();
			return;
		}

		if ( hint == _currentHint )
			return;

		DeleteHint();

		_currentHintPanel = hint.DisplayHint( UI.Hud.DisplayedPlayer );
		_currentHintPanel.Parent = UI.HintDisplay.Instance;
		_currentHintPanel.Enabled( true );

		_currentHint = hint;
	}

	private static void DeleteHint()
	{
		_currentHintPanel?.Delete();
		_currentHintPanel = null;

		_currentHint = null;
	}
}
