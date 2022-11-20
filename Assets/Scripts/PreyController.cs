using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : Entity
{
    private float preyForceMultiplier = 100;

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Prey_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Prey_ProliferationRate;
        PopUpSelf();
    }

    protected override void LifeTic()
    {
        //make decisions and live!

        //find closest Predator and move away from it
        GameObject target = FindClosestByTag("Predator");
        if (target != null)
        {
            rb.AddForce((transform.position - target.transform.position).normalized * preyForceMultiplier);  //move away
            BurnEnergy(0.1f);
        }
    }

    //handle Prey and Plant interaction
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
            if (target.CompareTag("Plant"))
            {
                //eat it!
                Consume(entity);
            }
            else if (target.CompareTag("Prey"))
            {
                TryReproduce(entity);
                BurnEnergy(storedEnergy * 0.01f);
            }
        }
    }
}
