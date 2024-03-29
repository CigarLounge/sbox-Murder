using Sandbox;
using Sandbox.UI;

namespace Murder;

public interface IEntityHint
{
	/// <summary>
	/// The max viewable distance of the hint.
	/// </summary>
	float HintDistance => Player.UseDistance;

	/// <summary>
	/// Whether or not we can show the UI hint.
	/// </summary>
	bool CanHint( Player player ) => true;

	/// <summary>
	/// The hint we should display.
	/// </summary>
	Panel DisplayHint( Player player )
	{
		return new UI.Hint() { HintText = DisplayInfo.For( (Entity)this ).Name };
	}
}
