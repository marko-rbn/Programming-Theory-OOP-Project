using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : Entity
{
    private float preyForceMultiplier = 300;

    //initialize
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Prey_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Prey_ProliferationRate;
        corpseDecaySeconds = DataManager.Instance.corpseDecaySeconds;
        resizingEntity = true;
        baseSize = 8;
        PopUpSelf();
    }

    //make decisions and live!
    //TODO: avoid Predators, seek Plants, roam around when not in range, seek mate when plenty of energy
    protected override void LifeTic()
    {
        //find closest Predator and move away from it
        GameObject target = FindClosestByTag("Predator", DataManager.Instance.sensoryRange);
        if (target != null)
        {
            AvoidTarget(target, preyForceMultiplier);
            AdjustEnergy(-0.1f);
        }
    }

    //handle Entity interactions
    private void OnCollisionEnter(Collision collision)
    {
        GameObject target = collision.collider.gameObject;
        if (target.CompareTag("Ground"))
        {
            isActive = true;  //enable normal activity after spawning
            return;
        }

        if (isActive && !isDead)
        {
            var entity = collision.collider.GetComponent<Entity>();
            if (target.CompareTag("Plant") && !entity.isDead)
            {
                Consume(entity);
            }
            else if (target.CompareTag("Prey") && !entity.isDead)
            {
                TryReproduce(entity);
            }
        }
    }
}
