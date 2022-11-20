using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FungusController : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Fungus_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Fungus_ProliferationRate;
        PopUpSelf(5f);
    }

    protected override void LifeTic()
    {
        //TODO: make decisions and live!
        //what does a fungus decide??
    }

    //handle Predator/Prey corpse interactions
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
                //eat it! - if corpse
                Consume(entity);
            }
        }
    }
}
