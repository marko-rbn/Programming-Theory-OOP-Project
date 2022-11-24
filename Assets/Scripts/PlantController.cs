using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantController : Entity
{
    //initialize
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Plant_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Plant_ProliferationRate;
        PopUpSelf(10f);
        actionMode = "photosynthesis";
    }

    //make decisions and live!
    //gain energy (photosynthesis) and reproduce
    protected override void LifeTic()
    {
        AdjustEnergy(DataManager.Instance.plantEnergyIncreasePerLifeTic);
        TryReproduce();
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
            if (target.CompareTag("Fungus"))
            {
                Consume(entity);
            }
        }
    }
}
