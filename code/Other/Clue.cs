using System.Collections.Generic;
using Sandbox;

namespace Murder;

public class Clue : Prop
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
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}
}
