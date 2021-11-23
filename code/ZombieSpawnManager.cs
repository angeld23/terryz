using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class ZombieSpawnManager
{
	public float MinZombieDist { get; set; }
	public float MaxZombieDist { get; set; }
	public int MinClusterSize { get; set; }
	public int MaxClusterSize { get; set; }
	public float ClusterRadius { get; set; }
	public int MaxZombies { get; set; }

	private float _timeStart;
	public float TimeExisted => Time.Now - _timeStart;

	public ZombieSpawnManager ()
	{
		Event.Register( this );

		_timeStart = Time.Now;

		MinZombieDist = 1250;
		MaxZombieDist = 3000;
		MinClusterSize = 1;
		MaxClusterSize = 7;
		ClusterRadius = 500;
		MaxZombies = 30;

		_prevTime = Time.Now;
	}

	~ZombieSpawnManager()
	{
		Event.Unregister( this );
	}

	// spawns a zombie at <pos>
	public Zombie SpawnZombieAtExactPoint( Vector3 pos )
	{
		var zombie = new Zombie();
		zombie.Position = pos;
		return zombie;
	}

	// spawns a zombie at the closest point on the navmesh to <pos>
	public Zombie SpawnZombie( Vector3 pos )
	{
		return SpawnZombieAtExactPoint(NavMesh.GetClosestPoint( pos ).GetValueOrDefault());
	}

	// tries its best to get a position that is at least MinZombieDist away from all players. (or gives the best outcome within maxRetries)
	public Vector3 GetRandomPosAwayFromPlayers( int maxRetries = 100 )
	{
		var playerEnumerable = Entity.All.OfType<Player>();

		var playerList = new List<Player>();
		foreach ( var player in playerEnumerable )
		{
			playerList.Add( player );
		}

		Vector3? bestPosFound = null;
		float bestDist = 0;

		for ( var i = 0; i < maxRetries; i++ )
		{
			// choose a random player to start off from
			var randomPlayer = Rand.FromList<Player>( playerList );

			// this shouldn't happen unless there are zero players in the game.
			// putting it in anyway
			if ( randomPlayer == null )
			{
				continue;
			}

			var _pos = NavMesh.GetPointWithinRadius(randomPlayer.Position, MinZombieDist, MaxZombieDist );
			if ( _pos == null )
			{
				continue;
			}
			var pos = _pos.GetValueOrDefault();

			var nobodyNearby = true;
			foreach ( var player in playerList )
			{
				var playerPos = player.Position;
				var playerDist = (playerPos - pos).Length;

				if ( bestPosFound == null || playerDist > bestDist )
				{
					bestPosFound = pos;
					bestDist = playerDist;
				}

				if ( playerDist < MinZombieDist )
				{
					nobodyNearby = false;
					break;
				}
			}

			if ( nobodyNearby )
			{
				return bestPosFound.GetValueOrDefault();
			}
		}

		// if we failed to find a perfect spot after the max amount of retries, just return the best we could get
		return bestPosFound.GetValueOrDefault();
	}

	public void AttemptSpawn()
	{
		var spawnOrigin = GetRandomPosAwayFromPlayers();
		var spawnCount = Rand.Int( MinClusterSize, MaxClusterSize );
		var zombieCount = Entity.All.OfType<Zombie>().Count();

		if ( zombieCount >= MaxZombies ) return;

		var game = (DeathmatchGame)Game.Current;
		game.PlaySoundFromWorld( "zombiespawn", spawnOrigin );

		for (var i = 0; i < spawnCount; i++)
		{
			if ( zombieCount >= MaxZombies ) break;

			SpawnZombie( NavMesh.GetPointWithinRadius( spawnOrigin, 0, ClusterRadius ) ?? spawnOrigin );
			zombieCount++;
		}
	}

	private float SpawnTimer = 0;
	private float _prevTime;
	[Event.Tick.Server]
	public virtual void Tick()
	{
		//Log.Info( TimeExisted );
		var timeDelta = Time.Now - _prevTime;

		SpawnTimer += timeDelta;
		if ( SpawnTimer > 5 )
		{
			SpawnTimer -= 5;
			AttemptSpawn();
		}

		_prevTime = Time.Now;
	}
}
