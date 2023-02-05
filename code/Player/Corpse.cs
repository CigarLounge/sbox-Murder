using Sandbox;
using Sandbox.UI;

namespace Murder;

[ClassName( "mur_ent_corpse" )]
[HideInEditor]
[Title( "Player corpse" )]
public partial class Corpse : ModelEntity, IEntityHint, IUse
{
	[Net] public Player Player { get; private set; }

	public Corpse() { }

	public Corpse( Player player )
	{
		Game.AssertServer();

		Player = player;
		Transform = player.Transform;
		Model = player.Model;

		this.CopyBonesFrom( player );
		this.SetRagdollVelocityFrom( player );
		ApplyForceToBone( Player.LastDamage.Force, Player.LastDamage.BoneIndex );
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
			clothing.Tags.Add( "corpse" );
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "interactable", "corpse" );

		PhysicsEnabled = true;
		UsePhysicsCollision = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		TakeDecalsFrom( Player );
		Player.Corpse = this;
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

	Panel IEntityHint.DisplayHint( Player player )
	{
		return new UI.Nameplate( Player );
	}

	bool IUse.OnUse( Entity user )
	{
		var murderer = (Player)user;

		if ( murderer.Clues > 0 )
		{
			murderer.Components.GetOrCreate<Disguise>().SetPlayer( Player );
			murderer.Clues--;
		}

		return false;
	}

	bool IUse.IsUsable( Entity user )
	{
		if ( GameState.Current is not GameplayState )
			return false;

		if ( user is not Player player )
			return false;

		if ( player.Role != Role.Murderer )
			return false;

		var isSame = player.BystanderName == Player.BystanderName;
		isSame &= player.Color == Player.Color;

		return !isSame;
	}
}
