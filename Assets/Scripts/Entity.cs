using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public bool isDead { get; protected set; }
    public bool isActive { get; protected set; }
    public bool isSelected { get; protected set; }
    public float storedEnergy { get; protected set; }  //spend energy to move and reproduce, gain energy from food or photosynthesis
    public int timeToLiveRemaining { get; protected set; }  //in seconds

    public string actionMode { get; protected set; }  //e.g.: hunt, roam, seek mate, etc.
    public float actionModeStarted { get; protected set; }  //start when current action mode started (in seconds since application start)
    public GameObject actionTarget { get; protected set; }  //actionMode specific target

    protected float proliferationRate;  //between 0 and 1
    protected int corpseDecaySeconds = 0;  //each child should set if non-zero
    protected bool resizingEntity = false;
    protected float baseSize;  //set in all children

    protected Rigidbody rb;
    protected MainManager mainManager;

    //don't override Awake in children
    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        rb = GetComponent<Rigidbody>();

        isDead = false;
        isActive = false;
        storedEnergy = DataManager.Instance.initialEntityEnergy;
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
        if (isSelected)
        {
            //adjust marker size
            mainManager.marker1Object.transform.localScale = transform.localScale * 0.15f;
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

    protected void SwitchMode(string newActionMode)
    {
        if (actionMode != newActionMode)
        {
            //different mode - reset timer
            actionModeStarted = Time.realtimeSinceStartup;
        }
        actionMode = newActionMode;
        SetActionTarget(null);
        //call LifeTic again, to initiate new Mode as soon as possible
        LifeTic();
    }

    //called when something attacks
    public void TriggerDeath()
    {
        if (isDead) return;  //can only die once ;)

        isDead = true;
        //storedEnergy = 0;

        StopMoving();
        SwitchMode("dead");

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
            mainManager.MarkTargetsTarget(null);
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

    //only Predator entity uses this (for now)
    protected void SetActionTarget(GameObject target)
    {
        actionTarget = target;
        //only allow selected entity request that mainManager mark its target
        if (isSelected)
        {
            mainManager.MarkTargetsTarget(target);
        }
    }

    //TODO: combine InterceptTarget and EvadeTarget so a single direction can be calculated for both
    //TODO: intercept direction as sum of target vector and direction vector
    protected void InterceptTarget(GameObject target, float forceMultiplier)
    {
        //direction vector to target
        Vector3 direction = (target.transform.position - transform.position);
        direction.y = 0;  //eliminate vertical component
        direction = direction.normalized;
        rb.AddForce(direction * forceMultiplier);
    }

    protected void EvadeTarget(GameObject target, float forceMultiplier)
    {
        Vector3 direction = (transform.position - target.transform.position);
        direction.y = 0;  //eliminate vertical component
        direction = direction.normalized;
        rb.AddForce(direction * forceMultiplier);
    }

    //TODO: prevent roaming too close to the edge... too many accidents
    protected void RandomRoam(float forceMultiplier)
    {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        //direction.y = 0;  //eliminate vertical component
        direction = rb.velocity * 0.5f + direction * 0.5f;  //retain 50% of current velocity and add 50% of random direction
        direction = direction.normalized;
        rb.AddForce(direction * forceMultiplier);
    }

    protected GameObject FindClosestByTag(string targetTag, float maxRange, bool live = true, string actionModeFilter = null)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float distanceSqr = Mathf.Infinity;
        Vector3 position = transform.position;
        float maxRangeSqr = maxRange * maxRange;
        foreach (GameObject go in gos)
        {
            //skip self
            if (go == gameObject) continue;

            var entity = go.GetComponent<Entity>();
            //live filter
            if (live && entity != null && entity.isDead) continue;
            //actionMode filter
            if (actionModeFilter != null && entity.actionMode != actionModeFilter) continue;

            //check distance
            Vector3 diff = go.transform.position - position;
            float curDistanceSqr = diff.sqrMagnitude;

            //out of range - skip
            if (curDistanceSqr > maxRangeSqr) continue;

            //find closest
            if (curDistanceSqr < distanceSqr)
            {
                closest = go;
                distanceSqr = curDistanceSqr;
            }
        }
        return closest;
    }

    protected float DistanceToTarget(GameObject target)
    {
        Vector3 v = target.transform.position - transform.position;
        return v.magnitude;
    }

    protected virtual void Consume(Entity target)
    {
        AdjustEnergy(target.storedEnergy * 0.6f);  //transfer half of target's energy to self
        target.storedEnergy *= 0.4f;  //NOTE: will limit how much Fungus can drain from corpses
        timeToLiveRemaining += DataManager.Instance.lifeClockBonusForFeeding;  //add bonus time
        target.TriggerDeath();  //kill target, if still alive
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
