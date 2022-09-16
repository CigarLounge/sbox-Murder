using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Murder;

[ClassName( "murder_entity_corpse" )]
[HideInEditor]
[Title( "Player corpse" )]
public partial class Corpse : ModelEntity, IEntityHint
{
	public Player Player { get; set; }

	public Corpse() { }

	public Corpse( Player player )
	{
		Host.AssertServer();

		Player = player;
		Owner = player;
		Transform = player.Transform;
		Model = player.Model;

		this.CopyBonesFrom( player );
		this.SetRagdollVelocityFrom( player );
		ApplyForceToBone( Player.LastDamage.Force, Player.GetHitboxBone( Player.LastDamage.HitboxIndex ) );
		CopyBodyGroups( player );

		foreach ( var child in Player.Children )
		{
			if ( !child.Tags.Has( "clothes" ) )
				continue;

			if ( child is not ModelEntity modelEntity )
				continue;

			var clothing = new ModelEntity
			{
				Model = modelEntity.Model,
				Position = modelEntity.Position,
				RenderColor = modelEntity.RenderColor
			};

			clothing.TakeDecalsFrom( modelEntity );
			clothing.CopyBodyGroups( modelEntity );
			clothing.CopyMaterialGroup( modelEntity );
			clothing.SetParent( this, true );
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "interactable" );

		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		TakeDecalsFrom( Owner as ModelEntity );
	}

	private void ApplyForceToBone( Vector3 force, int forceBone )
	{
		PhysicsGroup.AddVelocity( force );

		if ( forceBone < 0 )
			return;

		var corpse = GetBonePhysicsBody( forceBone );

		if ( corpse is not null )
			corpse.ApplyForce( force * 1000 );
		else
			PhysicsGroup.AddVelocity( force );
	}

	float IEntityHint.HintDistance => Player.MaxHintDistance;

	bool IEntityHint.CanHint( Player player ) => Game.Current.State is InProgress or PostRound;
}
