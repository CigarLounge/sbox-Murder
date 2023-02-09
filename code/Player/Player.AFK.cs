using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

public partial class Player
{
	private static readonly List<InputButton> _buttons = Enum.GetValues( typeof( InputButton ) ).Cast<InputButton>().ToList();
	private static TimeSince _timeSinceLastAction = 0f;

	/// <summary>
	/// Checks if we were active clientside. If not then we will
	/// start spectating.
	/// </summary>
	private void CheckAFK()
	{
		if ( Client.IsBot || Spectating.IsForced )
			return;

		if ( !this.IsAlive() )
		{
			_timeSinceLastAction = 0;
			return;
		}

		var isAnyKeyPressed = _buttons.Any( Input.Down );
		var isMouseMoving = Input.MouseDelta != Vector2.Zero;

		if ( isAnyKeyPressed || isMouseMoving )
		{
			_timeSinceLastAction = 0f;
			return;
		}

		if ( _timeSinceLastAction > GameManager.AFKTimer )
		{
			Spectating.IsForced = true;
			Input.StopProcessing = true;
		}
	}
}
