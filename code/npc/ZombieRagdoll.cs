using Sandbox;
using System;

public class ZombieRagdoll
{
	private const float fadeStartDelay = 5;
	private const float fadeTime = 2;
	private ModelEntity ragdoll;
	private float timeSpawned;

	public ZombieRagdoll( ModelEntity entity )
	{
		Event.Register( this );

		ragdoll = new ModelEntity( entity.GetModelName() );
		ragdoll.Transform = entity.Transform;
		ragdoll.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

		ragdoll.AngularVelocity = entity.AngularVelocity;
		ragdoll.RenderColor = entity.RenderColor;
		ragdoll.SetMaterialGroup( entity.GetMaterialGroup() );

		for ( var i = 0; i < ragdoll.BoneCount; i++ )
		{
			ragdoll.SetBoneTransform( i, entity.GetBoneTransform( i ) );
			var physBody = ragdoll.GetBonePhysicsBody( i );
			if ( physBody != null )
			{
				physBody.Velocity = -entity.Velocity * 30;
			}
		}

		ragdoll.SetInteractsAs( CollisionLayer.Debris );
		ragdoll.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ragdoll.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( Entity child in entity.Children )
		{
			if ( child is ModelEntity m )
			{
				new ModelEntity( m.GetModelName(), ragdoll );
			}
		}

		timeSpawned = Time.Now;
	}

	~ZombieRagdoll ()
	{
		Event.Unregister( this );
	}

	[Event.Tick.Server]
	public virtual void Tick()
	{
		var timeExisted = Time.Now - timeSpawned;

		if ( timeExisted > fadeTime + fadeStartDelay )
		{
			ragdoll.Delete();
			return;
		}

		var alpha = timeExisted.LerpInverse( fadeStartDelay + fadeTime, fadeStartDelay );
		var c = ragdoll.RenderColor;
		ragdoll.RenderColor = new Color(c.r, c.g, c.b, alpha );
		foreach ( Entity child in ragdoll.Children )
		{
			if ( child is ModelEntity m )
			{
				var c2 = m.RenderColor;
				m.RenderColor = new Color( c2.r, c2.g, c2.b, alpha );
			}
		}
	}
}
