using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : Entity
{
    private float predatorForceMultiplier = 300;

    //initialize
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Predator_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Predator_ProliferationRate;
        corpseDecaySeconds = DataManager.Instance.corpseDecaySeconds;
        resizingEntity = true;
        baseSize = 10;
        PopUpSelf();
        UpdateEntitySize();
    }

    //make decisions and live!
    //TODO: follow and kill Prey, roam around when not in range, seek mate when plenty of energy
    protected override void LifeTic()
    {
        //find closest Prey and move to it
        GameObject target = FindClosestByTag("Prey", DataManager.Instance.sensoryRange);
        if (target != null && isActive)
        {
            InterceptTarget(target, predatorForceMultiplier);
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
            if (target.CompareTag("Prey") && !entity.isDead)
            {
                Consume(entity);
            }
            else if (target.CompareTag("Predator") && !entity.isDead)
            {
                TryReproduce(entity);
            }
        }
    }
}
