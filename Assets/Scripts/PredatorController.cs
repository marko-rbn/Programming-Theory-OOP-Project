using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : Entity
{
    private float predatorForceMultiplier = 100;

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Predator_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Predator_ProliferationRate;
        PopUpSelf();
    }

    protected override void LifeTic()
    {
        //make decisions and live!

        //find closest Prey and move to it
        GameObject target = FindClosestByTag("Prey");
        if (target != null)
        {
            rb.AddForce((target.transform.position - transform.position).normalized * predatorForceMultiplier);  //move toward
            BurnEnergy(0.1f);
        }
    }

    //handle Predator and Prey interactions
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
                //consume prey, if it's alive - gain energy
                Consume(entity);
            }
            else if (target.CompareTag("Predator") && !entity.isDead)
            {
                //fight another predator if alive - subtract 1% energy from each
                TryReproduce(entity);
            }
        }
    }
}
