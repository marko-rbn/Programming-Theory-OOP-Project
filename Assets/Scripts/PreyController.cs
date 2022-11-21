using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : Entity
{
    private float preyForceMultiplier = 100;
    private float baseSize = 8;

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
            AdjustEnergy(-0.1f);
        }
    }

    //called after energy change
    protected override void UpdateEntitySize()
    {
        float adjustedSize = baseSize + storedEnergy * 0.05f;
        transform.localScale = Vector3.one * adjustedSize;
        float minHeight = adjustedSize / 2;
        if (transform.position.y < minHeight)
        {
            transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
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
                AdjustEnergy(-storedEnergy * 0.01f);
            }
        }
    }
}
