using Sandbox;

namespace Murder;

[ClassName( "murder_entity_corpse" )]
[HideInEditor]
[Title( "Player corpse" )]
public partial class Corpse : ModelEntity, IEntityHint, IUse
{
	[Net]
	public Player Player { get; private set; }

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

	UI.EntityHintPanel IEntityHint.DisplayHint( Player player )
	{
		return new UI.Nameplate( Player );
	}

	bool IUse.OnUse( Entity user )
	{
		var murderer = (Player)user;

		murderer.Components.GetOrCreate<Disguise>().SetPlayer( Player );

		return false;
	}

	bool IUse.IsUsable( Entity user )
	{
		if ( Game.Current.State is not GameplayState )
			return false;

		if ( user is not Player player )
			return false;

		return player.Role == Role.Murderer;
	}
}
