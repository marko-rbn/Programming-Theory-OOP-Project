using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [SerializeField]
    public bool isDead { get; protected set; }
    [SerializeField]
    public bool isActive { get; protected set; }
    [SerializeField]
    public float storedEnergy { get; protected set; }  //spend energy to move and reproduce, gain energy from food or photosynthesis

    [SerializeField]
    protected int timeToLiveRemaining;  //in seconds
    [SerializeField]
    protected float proliferationRate;  //between 0 and 1

    protected Rigidbody rb;
    private MainManager mainManager;

    //don't override Awake
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

    private void Decay()
    {
        Destroy(gameObject);
    }

    //TODO: rename to AdjustEnergy(), use energy level to resize the entity
    protected void BurnEnergy(float amount)
    {
        storedEnergy -= amount;
        if (storedEnergy <= 0)
        {
            storedEnergy = 0;
            OnDeath();  //ran out of energy
        }
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
        storedEnergy += (target.storedEnergy / 2);  //transfer half of target's energy to self
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
            BurnEnergy(50);
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
            BurnEnergy(50);
            mainManager.SpawnOne(gameObject, false);
        } else
        {
            BurnEnergy(storedEnergy * 0.01f);  //1% for the attempt
        }
    }

    //runs every second, use in child classes to make decisions about current actions and mode of existance
    protected abstract void LifeTic();
}
