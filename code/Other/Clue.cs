using Sandbox;
using Sandbox.Component;
using Editor;

namespace Murder;

[ClassName( "mur_ent_clue" )]
[Description( "A clue that will randomly spawn during gameplay." )]
[HammerEntity]
[Title( "Clue" )]
public class Clue : Prop, IUse
{
	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "ignorereset" );

		PhysicsEnabled = false;

		var glow = Components.GetOrCreate<Glow>();

		glow.Color = Color.Green;
		glow.ObscuredColor = Color.Transparent;
		glow.Enabled = true;

		Hide();
	}

	internal void Show()
	{
		EnableDrawing = true;
		EnableAllCollisions = true;
		UsePhysicsCollision = true;
	}

	[Event.Entity.PostCleanup]
	internal void Hide()
	{
		EnableDrawing = false;
		EnableAllCollisions = false;
		UsePhysicsCollision = false;
	}

	bool IUse.IsUsable( Entity user )
	{
		return user is Player;
	}

	bool IUse.OnUse( Entity user )
	{
		var player = (Player)user;

		player.CluesCollected++;

		if ( player.Role != Role.Murderer )
		{
			if ( player.CluesCollected % 5 == 0 )
				player.SetCarriable( new Revolver(), true );
		}

		Sound.FromScreen( To.Single( player ), "clue_collected" );

		Hide();

		return false;
	}
}
