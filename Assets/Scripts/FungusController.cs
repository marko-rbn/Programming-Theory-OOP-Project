using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungusController : Entity
{
    //initialize
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Fungus_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Fungus_ProliferationRate;
        PopUpSelf(5f);
    }

    //make decisions and live!
    //TODO: reproduce when near corpses
    protected override void LifeTic()
    {
        
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
            if ((target.CompareTag("Predator") || target.CompareTag("Prey")) && entity.isDead)
            {
                Consume(entity);
            }
        }
    }
}
