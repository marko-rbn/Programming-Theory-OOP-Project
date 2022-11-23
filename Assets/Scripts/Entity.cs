using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public bool isDead { get; protected set; }
    public bool isActive { get; protected set; }
    public bool isSelected { get; protected set; }
    public float storedEnergy { get; protected set; }  //spend energy to move and reproduce, gain energy from food or photosynthesis
    public string actionMode { get; protected set; }
    public int timeToLiveRemaining { get; protected set; }  //in seconds

    protected float proliferationRate;  //between 0 and 1
    protected int corpseDecaySeconds = 0;  //each child should set if non-zero
    protected bool resizingEntity = false;
    protected float baseSize;  //set in all children

    protected Rigidbody rb;
    private MainManager mainManager;

    //don't override Awake in children
    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        rb = GetComponent<Rigidbody>();
        isDead = false;
        isActive = false;
        storedEnergy = DataManager.Instance.initialEntityEnergy;
        actionMode = "TBD";  //TODO: store higher level mode of operation: chasing, resting, reproducing, etc.
        InvokeRepeating("Agify", 1, 1);
    }

    protected void PopUpSelf(float dispersalAngle = 0)
    {
        float xDeviation = Random.Range(-dispersalAngle, dispersalAngle);
        float zDeviation = Random.Range(-dispersalAngle, dispersalAngle);
        Vector3 popDirection = new Vector3(xDeviation, 1, zDeviation).normalized;
        rb.AddForce(popDirection * 1000);  //upward push
    }
    
    //kill entities falling out of the box
    private void Update()
    {
        if (transform.position.y < 0 && !isDead) TriggerDeath();
    }

    //called after energy change
    protected void UpdateEntitySize()
    {
        float adjustedSize = baseSize + storedEnergy * 0.05f;
        transform.localScale = Vector3.one * adjustedSize;
        float minHeight = adjustedSize / 2;
        if (transform.position.y < minHeight)
        {
            transform.position = new Vector3(transform.position.x, minHeight, transform.position.z);
        }
    }

    // tick down the life counter
    private void Agify()
    {
        timeToLiveRemaining--;
        if (timeToLiveRemaining <= 0)
        {
            TriggerDeath();
        } else
        {
            LifeTic();
        }
    }

    //called when something attacks
    public void TriggerDeath()
    {
        if (isDead) return;  //can only die once ;)

        isDead = true;
        storedEnergy = 0;

        StopMoving();

        //and die
        var renderer = GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.black);

        CancelInvoke("Agify");  //stop aging

        mainManager.AfterEntityDeath(this);  //let manager know

        Invoke("Decayed", corpseDecaySeconds);
    }

    //left click to select, click again to deselect
    private void OnMouseDown()
    {
        if (isSelected)
        {
            mainManager.SelectEntity(null);  //Deselect() is called from mainManager
        } else
        {
            mainManager.SelectEntity(this);
            isSelected = true;
        }
    }

    public void Deselect()
    {
        isSelected = false;
    }

    private void Decayed()
    {
        if (isSelected)
        {
            mainManager.SelectEntity(null);  //deselect before destroyed
        }
        mainManager.AfterEntityDecay(gameObject.tag);
        Destroy(gameObject);
    }

    protected void AdjustEnergy(float amount)
    {
        storedEnergy += amount;
        if (resizingEntity) UpdateEntitySize();
        if (storedEnergy <= 0)
        {
            storedEnergy = 0;
            TriggerDeath();  //ran out of energy
        }
    }

    protected void StopMoving()
    {
        rb.velocity = new Vector3(0, rb.velocity.y, 0);  //keep vertical velocity, but not horizontal
        rb.angularVelocity = Vector3.zero;
    }

    //TODO: combine InterceptTarget and AvoidTarget so a single direction can be calculated for both
    protected void InterceptTarget(GameObject target, float forceMultiplier)
    {
        //TODO: intercept direction?
        Vector3 direction = (target.transform.position - transform.position);
        direction.y = 0;  //eliminate vertical component
        direction = direction.normalized;
        rb.AddForce(direction * forceMultiplier);  //move toward
    }

    protected void AvoidTarget(GameObject target, float forceMultiplier)
    {
        Vector3 direction = (transform.position - target.transform.position);
        direction.y = 0;  //eliminate vertical component
        direction = direction.normalized;
        rb.AddForce(direction * forceMultiplier);  //move toward
    }

    protected GameObject FindClosestByTag(string targetTag, float maxRange, bool live = true)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        float maxRangeSqr = maxRange * maxRange;
        foreach (GameObject go in gos)
        {
            if (live) {
                //if live is required, skip all the corpses
                var entity = go.GetComponent<Entity>();
                if (entity != null && entity.isDead) continue;
            }

            Vector3 diff = go.transform.position - position;
            float curDistanceSqr = diff.sqrMagnitude;

            if (curDistanceSqr > maxRangeSqr)
            {
                //outside of sensory range, ignore this one
                continue;
            }

            if (curDistanceSqr < distance)
            {
                closest = go;
                distance = curDistanceSqr;
            }
        }
        return closest;
    }

    protected virtual void Consume(Entity target)
    {
        AdjustEnergy(target.storedEnergy / 2);  //transfer half of target's energy to self
        timeToLiveRemaining += DataManager.Instance.lifeClockBonusForFeeding;  //add bonus time
        target.TriggerDeath();  //kill target
    }

    //Polymorphism ;)
    //Plant and Fungus don't need another to reproduce
    protected virtual void TryReproduce()
    {
        bool successByChance = Random.Range(0f, 1f) < proliferationRate;
        if (storedEnergy >= 100 && successByChance)
        {
            //ask manager to spawn another of same type
            AdjustEnergy(-50);
            mainManager.SpawnOne(gameObject, false);
        }
    }

    //Polymorphism ;)
    //Predator and Prey require another (target) to reproduce
    protected virtual void TryReproduce(Entity target)
    {
        bool successByChance =  Random.Range(0f, 1f) < proliferationRate;
        if (storedEnergy >= 100 && target.storedEnergy >= 100 && successByChance)
        {
            //ask manager to spawn another of same type - main manager may refuse if there are too many
            if (mainManager.SpawnOne(gameObject, false))
            {
                AdjustEnergy(-50);
            }
        } else
        {
            AdjustEnergy(-storedEnergy * 0.01f);  //1% for the attempt
        }
    }

    //runs every second, use in child classes to make decisions about current actions and mode of operation
    protected abstract void LifeTic();
}
