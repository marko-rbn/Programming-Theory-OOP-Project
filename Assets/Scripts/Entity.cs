using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField]
    protected int timeToLiveRemaining;  //in seconds
    [SerializeField]
    protected float proliferationRate;  //between 0 and 1
    [SerializeField]
    private bool isDead = false;
    [SerializeField]
    private int storedEnergy = 100;  //spend energy to move, get energy from food

    private MainManager mainManager;

    private void Awake()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        InvokeRepeating("Agify", 1, 1);
    }

    // tick down the life counter
    void Agify()
    {
        timeToLiveRemaining--;
        if (timeToLiveRemaining == 0)
        {
            isDead = true;

            //Do some gross stuff - turn flat and black, and wait for fungus
            gameObject.transform.localScale.Scale(Vector3.up / 2);
            var renderer = GetComponent<Renderer>();
            renderer.material.SetColor("_Color", Color.black);
            mainManager.OnDeath(this);

            CancelInvoke("Agify");
        }
    }

    // override in child class
    protected virtual void Consume(Entity target)
    {
    }

    protected virtual void Reproduce()
    {
    }

    protected virtual void Move(Entity towardTarget)
    {
    }
}
