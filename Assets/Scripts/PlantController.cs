using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Plant_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Plant_ProliferationRate;
        PopUpSelf(10f);
    }

    protected override void LifeTic()
    {
        //make decisions and live!

        //increase energy (photosynthesis?)
        AdjustEnergy(DataManager.Instance.plantEnergyIncreasePerLifeTic);

        //what does a plant decide??
        TryReproduce();
    }

    //handle Fungus interaction
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
            if (target.CompareTag("Fungus"))
            {
                //eat it!
                Consume(entity);
            }
        }
    }
}
