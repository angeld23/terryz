using Sandbox;
using System;

/// <summary>
/// This is the heart of the gamemode. It's responsible
/// for creating the player and stuff.
/// </summary>
partial class DeathmatchGame : Game
{
	[ClientRpc]
	public void PlaySoundFromScreen( string name, double x = 0.5, double y = 0.5 )
	{
		Sound.FromScreen( name, (float)x, (float)y );
	}

	[ClientRpc]
	public void PlaySoundFromWorld( string name, Vector3 position )
	{
		Sound.FromWorld( name, position );
	}

	[ClientRpc]
	public void PlaySoundFromEntity( string name, Entity entity )
	{
		Sound.FromEntity( name, entity );
	}

	public DeathmatchHud Hud;
	public DeathmatchGame()
	{
		//
		// Create the HUD entity. This is always broadcast to all clients
		// and will create the UI panels clientside. It's accessible 
		// globally via Hud.Current, so we don't need to store it.
		//
		if ( IsServer )
		{
			Hud = new DeathmatchHud();
			new ZombieSpawnManager();
		}
	}

	public override void PostLevelLoaded()
	{
		base.PostLevelLoaded();

		ItemRespawn.Init();
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		var player = new DeathmatchPlayer();
		player.UpdateClothes( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	private bool playedMusic;
	public override void Simulate( Client cl )
	{
		if ( !playedMusic && IsClient && Input.Pressed( InputButton.Forward ) )
		{
			playedMusic = true;
			Sound.FromScreen( "zombiemusic" );
		}
		base.Simulate(cl);
	}
}
