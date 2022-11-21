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
    
    private void Update()
    {
        if (transform.position.y < 0) Destroy(gameObject);  //if it ever ends up below the ground plane, destroy it
    }

    // tick down the life counter
    private void Agify()
    {
        timeToLiveRemaining--;
        if (timeToLiveRemaining <= 0)
        {
            OnDeath();
        } else
        {
            LifeTic();
        }
    }

    //called when something attacks
    public void OnDeath()
    {
        isDead = true;
        storedEnergy = 0;

        //stop
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //drop
        //TODO: animate the drop, replace mesh and collider with cylinder

        //and die
        var renderer = GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.black);

        CancelInvoke("Agify");  //stop aging

        mainManager.OnEntityDeath(this);  //let manager know

        //TODO: probably try to move this code to child classes, since it's specific
        if (gameObject.CompareTag("Predator") || gameObject.CompareTag("Prey"))
        {
            Invoke("Decay", DataManager.Instance.corpseDecaySeconds);
        } else
        {
            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        //left click to select, click again to deselect
        if (isSelected)
        {
            mainManager.SelectEntity(null);  //Deselect() is called from mainManager
        } else
        {
            mainManager.SelectEntity(this);
            isSelected = true;
            //TODO: enable mark
        }
    }

    public void Deselect()
    {
        isSelected = false;
        //TODO: disable mark
    }

    private void Decay()
    {
        if (isSelected)
        {
            mainManager.SelectEntity(null);  //deselect before destroyed
        }
        Destroy(gameObject);
    }

    //TODO: use energy level to resize the entity
    protected void AdjustEnergy(float amount)
    {
        storedEnergy += amount;
        UpdateEntitySize();
        if (storedEnergy <= 0)
        {
            storedEnergy = 0;
            OnDeath();  //ran out of energy
        }
    }

    protected virtual void UpdateEntitySize()
    {
        //override in child classes to adjust size based on energy stored and type of entity
        //by default no change
    }

    protected GameObject FindClosestByTag(string targetTag, bool live = true)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            if (live) {
                //if live is required, skip all the corpses
                var entity = go.GetComponent<Entity>();
                if (entity != null && entity.isDead) continue;
            }

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

    protected virtual void Consume(Entity target)
    {
        AdjustEnergy(target.storedEnergy / 2);  //transfer half of target's energy to self
        timeToLiveRemaining += DataManager.Instance.lifeClockBonusForFeeding;  //add bonus time
        target.OnDeath();  //kill target
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
            //ask manager to spawn another of same type
            AdjustEnergy(-50);
            mainManager.SpawnOne(gameObject, false);
        } else
        {
            AdjustEnergy(-storedEnergy * 0.01f);  //1% for the attempt
        }
    }

    //runs every second, use in child classes to make decisions about current actions and mode of existance
    protected abstract void LifeTic();
}
