using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : Entity
{
    private float preyForceMultiplier = 300;

    void Start()
    {
        timeToLiveRemaining = DataManager.Instance.settings.Prey_MaxLifespan;
        proliferationRate = DataManager.Instance.settings.Prey_ProliferationRate;
        corpseDecaySeconds = DataManager.Instance.corpseDecaySeconds;
        resizingEntity = true;
        baseSize = 8;

        PopUpSelf();
        UpdateEntitySize();
    }

    //make decisions and live!
    //actionModes: graze, seek-mate, roam, dead
    //TODO: fix issue with "roam", where it resets the time counter every two sec, because of switching to other modes
    //TODO: combining InterceptTarget and EvadeTarget can simplify the logic for Prey
    protected override void LifeTic()
    {
        float secondsSinceModeChange = Time.realtimeSinceStartup - actionModeStarted;
        switch (actionMode)
        {
            case "graze":
                if ((secondsSinceModeChange > 15 && actionTarget == null) && storedEnergy > 200)
                {
                    //been grazing, no plants, and pretty full
                    SwitchMode("seek-mate");
                }
                else
                {
                    //continue grazing but watch for Predators
                    if (actionTarget == null)
                    {
                        //find closest Predator and move away from it
                        actionTarget = FindClosestByTag("Predator", DataManager.Instance.sensoryRange);
                        if (actionTarget != null)
                        {
                            float distance = DistanceToTarget(actionTarget);
                            if (distance < 50)
                            {
                                //too close for comfort - evade
                                EvadeTarget(actionTarget, preyForceMultiplier);
                                AdjustEnergy(-0.1f);
                            }
                            else
                            {
                                actionTarget = null;  //unset target, so a plant can be targetted instead
                            }
                        }
                    }
                    if (actionTarget == null)
                    {
                        //acquire target - closest Plant
                        actionTarget = FindClosestByTag("Plant", DataManager.Instance.sensoryRange);
                        if (actionTarget == null)
                        {
                            SwitchMode("roam");  //back to roam if no nearby Prey
                            break;
                        }
                    }
                    //move toward plant
                    if (actionTarget != null && isActive)
                    {
                        float distance = DistanceToTarget(actionTarget);
                        if (actionTarget.GetComponent<Entity>().isDead || distance > (DataManager.Instance.sensoryRange * 2))
                        {
                            //if target dead or moved out of range (twice sensory range) - unset target
                            actionTarget = null;
                        }
                        else
                        {
                            InterceptTarget(actionTarget, preyForceMultiplier);
                            AdjustEnergy(-0.1f);
                        }
                    }
                }
                break;
            case "seek-mate":
                if ((secondsSinceModeChange > 10 && actionTarget == null) || storedEnergy < 200)
                {
                    //been looking, can't find, getting hungry
                    SwitchMode("graze");
                }
                else
                {
                    //continue seeking mate
                    if (actionTarget == null)
                    {
                        //acquire target - closest Prey that is also seeking mate
                        actionTarget = FindClosestByTag("Prey", 1000, true, "seek-mate");  //increase range for seeking mate
                        if (actionTarget == null && secondsSinceModeChange > 10)
                        {
                            SwitchMode("roam");  //back to roam, if no nearby mates and been looking a while
                            break;
                        }
                    }
                    //pursue
                    if (actionTarget != null && isActive)
                    {
                        InterceptTarget(actionTarget, preyForceMultiplier);
                        AdjustEnergy(-0.1f);
                    }
                }
                break;
            case "roam":
                if (secondsSinceModeChange > 2)
                {
                    //try the other two modes
                    SwitchMode((storedEnergy > 200) ? "seek-mate" : "graze");
                }
                else
                {
                    //continue roaming
                    RandomRoam(preyForceMultiplier * 0.75f);
                    AdjustEnergy(-0.1f);
                }
                break;
            case "dead":
                //do nothing
                break;
            default:
                //no mode selected yet
                SwitchMode("graze");
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
            if (target.CompareTag("Plant") && !entity.isDead)
            {
                Consume(entity);
            }
            else if (target.CompareTag("Prey") && !entity.isDead)
            {
                TryReproduce(entity);
            }
        }
    }
}
