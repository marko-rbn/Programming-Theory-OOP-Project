using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : Entity
{
    private float predatorSpeed = 100;

    // Start is called before the first frame update
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Predator_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Predator_ProliferationRate;
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void LifeTic()
    {
        //TODO: make decisions and live!

        //TEMP: find closest Prey and move to it
        GameObject target = FindClosestByTag("Prey");
        rb.AddForce((target.transform.position - transform.position).normalized * predatorSpeed);
        storedEnergy -= 0.1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject target = collision.collider.gameObject;
        var entity = collision.collider.GetComponent<Entity>();
        if (target.CompareTag("Prey"))
        {
            //TODO: eat it!
            //Debug.Log("Eat it!");
            //Debug.Log("Prey entity: " + entity);
            //Consume(entity);
            Destroy(target);
        }
        if (target.CompareTag("Predator"))
        {
            //TODO: eat it!
            Debug.Log("Eat it!");
            Debug.Log("Predator entity: " + entity);
            Debug.Log("energy: " + entity.storedEnergy);
            Consume(entity);
            Destroy(target);
        }
    }

    protected override void Consume(Entity target)
    {
        //Entity entity = target.GetComponent<Entity>();
        Debug.Log("energy: " + target.storedEnergy);
        //Destroy(target);
    }

    protected override void Reproduce()
    {
    }

    protected override void Chase(GameObject target)
    {
    }

}
