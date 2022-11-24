using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : Entity
{
    private float predatorForceMultiplier = 300;

    //initialize
    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Predator_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Predator_ProliferationRate;
        corpseDecaySeconds = DataManager.Instance.corpseDecaySeconds;
        resizingEntity = true;
        baseSize = 10;
        PopUpSelf();
        UpdateEntitySize();
    }

    //make decisions and live!
    //actionModes: hunt, seek-mate, roam, dead
    //TODO: fix issue with "roam", where it resets the time counter every two sec, because of switching to other modes
    protected override void LifeTic()
    {
        float secondsSinceModeChange = Time.realtimeSinceStartup - actionModeStarted;
        switch (actionMode)
        {
            case "hunt":
                if ((secondsSinceModeChange > 15 && actionTarget == null) && storedEnergy > 200)
                {
                    //been hunting, no victims, and pretty full
                    SwitchMode("seek-mate");
                } else
                {
                    //continue hunt
                    if (actionTarget == null)
                    {
                        //acquire target - closest Prey
                        actionTarget = FindClosestByTag("Prey", DataManager.Instance.sensoryRange);
                        if (actionTarget == null)
                        {
                            SwitchMode("roam");  //back to roam if no nearby Prey
                            break;
                        }
                    }
                    //pursue
                    if (actionTarget != null && isActive)
                    {
                        float distance = DistanceToTarget(actionTarget);
                        if (actionTarget.GetComponent<Entity>().isDead || distance > (DataManager.Instance.sensoryRange * 2))
                        {
                            //if target dead or moved out of range (twice sensory range) - unset target
                            actionTarget = null;
                        } else
                        {
                            InterceptTarget(actionTarget, predatorForceMultiplier);
                            AdjustEnergy(-0.1f);
                        }
                    }
                }
                break;
            case "seek-mate":
                if ((secondsSinceModeChange > 10 && actionTarget == null) || storedEnergy < 200)
                {
                    //been looking, can't find, getting hungry
                    SwitchMode("hunt");
                } else
                {
                    //continue seeking mate
                    if (actionTarget == null)
                    {
                        //acquire target - closest Predator that is also seeking mate
                        actionTarget = FindClosestByTag("Predator", 1000, true, "seek-mate");  //increase range for seeking mate
                        if (actionTarget == null && secondsSinceModeChange > 10)
                        {
                            SwitchMode("roam");  //back to roam, if no nearby mates and been looking a while
                            break;
                        }
                    }
                    //pursue
                    if (actionTarget != null && isActive)
                    {
                        InterceptTarget(actionTarget, predatorForceMultiplier);
                        AdjustEnergy(-0.1f);
                    }
                }
                break;
            case "roam":
                if (secondsSinceModeChange > 2)
                {
                    //try the other two modes
                    SwitchMode((storedEnergy > 200) ? "seek-mate" : "hunt");
                } else
                {
                    //continue roaming
                    RandomRoam(predatorForceMultiplier * 0.75f);
                    AdjustEnergy(-0.1f);
                }
                break;
            case "dead":
                //do nothing
                break;
            default:
                //no mode selected yet
                SwitchMode("hunt");
                break;
        }
    }

    //handle Entity interactions
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
            if (target.CompareTag("Prey") && !entity.isDead)
            {
                Consume(entity);
            }
            else if (target.CompareTag("Predator") && !entity.isDead)
            {
                TryReproduce(entity);
                SwitchMode("hunt");  //hopefully success, can go back to hunt
            }
        }
    }
}
