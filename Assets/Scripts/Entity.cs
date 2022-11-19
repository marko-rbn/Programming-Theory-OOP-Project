using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField]
    protected int timeToLiveRemaining;  //in seconds
    [SerializeField]
    protected float proliferationRate;  //between 0 and 1
    [SerializeField]
    public bool isDead { get; private set; }

    protected Rigidbody rb;
    [SerializeField]
    public float storedEnergy { get; protected set; }  //spend energy to move, get energy from food

    private MainManager mainManager;

    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        rb = GetComponent<Rigidbody>();
        isDead = false;
        storedEnergy = 50;
        InvokeRepeating("Agify", 1, 1);
    }

    // tick down the life counter
    void Agify()
    {
        timeToLiveRemaining--;
        if (timeToLiveRemaining <= 0)
        {
            isDead = true;

            //stop
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            //drop
            //FlattenMe();

            //and die
            var renderer = GetComponent<Renderer>();
            renderer.material.SetColor("_Color", Color.black);

            CancelInvoke("Agify");  //stop aging

            mainManager.OnDeath(this);  //let manager know
        } else
        {
            LifeTic();
        }
    }

    private void FlattenMe()
    {
        //TODO: animate the drop, replace mesh and collider with cylinder
        //NOTE: not working for now
        float amount = 4;
        transform.position += Vector3.down * amount / 2; // Move the object in the direction of scaling, so that the corner on ther side stays in place
        transform.localScale += Vector3.down * amount; // Scale object in the specified direction
    }

    protected GameObject FindClosestByTag(string targetTag)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    // override in child class

    //runs every second, use in child classes to make decisions about current actions and mode of existance
    protected abstract void LifeTic();

    protected virtual void Consume(Entity target)
    {
    }

    protected virtual void Reproduce()
    {
    }

    protected virtual void Chase(GameObject towardTarget)
    {
    }
}
