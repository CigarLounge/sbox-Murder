using Editor;
using Sandbox;
using Sandbox.Component;
using System;

namespace Murder;

[ClassName( "mur_clue" )]
[Description( "A clue that will randomly spawn during gameplay." )]
[HammerEntity]
[Title( "Clue" )]
public class Clue : Prop, IUse
{
	[ConVar.Server( "mur_clue_spawnrate", Help = "Time (in seconds) it takes for a clue to spawn.", Saved = true )]
	public static int SpawnRate { get; set; } = 7;

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "ignorereset" );

		PhysicsEnabled = false;

		var glow = Components.GetOrCreate<Glow>();

		glow.Color = Color.Green;
		glow.ObscuredColor = Color.Transparent;

		Hide();
	}

	internal void Show()
	{
		EnableDrawing = true;
		EnableAllCollisions = true;
		UsePhysicsCollision = true;
	}

	[Event.Entity.PostCleanup]
	[GameEvent.Round.End]
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

		player.Clues++;
		player.CluesCollected++;

		if ( player.Role == Role.Bystander && player.Clues % 5 == 0 )
		{
			player.SetCarriable( new Revolver(), true );
			player.Clues = 0;
		}

		player.Clues = Math.Min( player.Clues, 5 );

		Sound.FromScreen( To.Single( player ), "clue_collected" );

		Hide();

		return false;
	}
}
