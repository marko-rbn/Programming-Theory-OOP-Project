using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : Entity
{
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

    private void OnCollisionEnter(Collision collision)
    {
        //TODO: eat if food!
    }

    protected override void Consume(Entity target)
    {
    }

    protected override void Reproduce()
    {
    }

    protected override void Move(Entity towardTarget)
    {
    }

}
