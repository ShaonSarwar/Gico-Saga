using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

//[ExecuteInEditMode]
public class KillParticle : MonoBehaviour
{
    private GridLogicVisual gridLogicVisual;
    private void Awake()
    {
        gridLogicVisual = GameObject.Find("GridLogicVisual").GetComponent<GridLogicVisual>(); 
    }
    private void OnParticleTrigger()
    {
        if (!gameObject.activeInHierarchy) return; 
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
       
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

        int numEnter = ParticlePhysicsExtensions.GetTriggerParticles(particleSystem, ParticleSystemTriggerEventType.Enter, enter);
        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];

            // Identifing Which Collider Triggered with Particles 
            for (int j = 0; j < particleSystem.trigger.colliderCount; j++)
            {
                CircleCollider2D collider = particleSystem.trigger.GetCollider(j).GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    Transform spriteTransform = collider.transform.Find("Sprite");
                    Vector3 colliderSpritePosition = new Vector3(spriteTransform.position.x, spriteTransform.position.y, 0); 
                    if (IsPointInsideTheCollider(colliderSpritePosition, new Vector3(p.position.x, p.position.y, 0), collider.radius))
                    {
                        gridLogicVisual.TryDemageInsulator(colliderSpritePosition);
                    }
                }
               
            }
            p.remainingLifetime = 0;
            enter[i] = p;
        }
        ParticlePhysicsExtensions.SetTriggerParticles( particleSystem, ParticleSystemTriggerEventType.Enter, enter);
    }

    public bool IsPointInsideTheCollider(Vector3 colliderCentre, Vector3 point, float colliderRadius)
    {
        float distance = (new Vector3(colliderCentre.x, colliderCentre.y, 0) - point).magnitude;
        Vector3 dist = (new Vector3(colliderCentre.x, colliderCentre.y, 0) - point).normalized * distance; 
        if ( dist.magnitude <= colliderRadius)
        {
            return true; 
        }
        return false; 
    }
}
