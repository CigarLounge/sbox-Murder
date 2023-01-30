using Sandbox;
using Sandbox.Component;
using System.Collections.Generic;

namespace Murder;

public class Clue : Prop, IUse
{
	private static readonly List<Model> _models = new()
	{
		Model.Load( "models/sbox_props/bin/rubbish_bag.vmdl" ),
		Model.Load( "models/sbox_props/bin/rubbish_can.vmdl" ),
		Model.Load( "models/sbox_props/bin/pizza_box.vmdl" ),
		Model.Load( "models/sbox_props/grit_box/grit_box.vmdl" ),
	};

	public override void Spawn()
	{
		Model = Game.Random.FromList( _models );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		PhysicsEnabled = true;
		UsePhysicsCollision = true;

		var glow = new Glow
		{
			Color = Color.Green,
			ObscuredColor = Color.Transparent,
		};

		Components.Add( glow );
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
			if ( player.CluesCollected == 5 )
				player.SetCarriable( new Revolver() );
			else if ( player.CluesCollected % 15 == 0 )
				player.SetCarriable( new Revolver() );
		}

		PlaySound( "clue_collected" );
		Delete();

		return false;
	}
}
