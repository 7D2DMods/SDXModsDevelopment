using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetIfHurtSDX : EAISetAsTargetIfHurt
{
    private List<Entity> NearbyEntities = new List<Entity>();

    private bool blDisplayLog = true;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }


    public override bool CanExecute()
    {
        DisplayLog(" CanExecute?");
        // Grab the current revent target. 
        EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
        if (revengeTarget)
        {
            DisplayLog(" Revenge Target is: " + revengeTarget.EntityName);
            // No zombie on zombie fighting.
            if (revengeTarget as EntityZombie && this.theEntity as EntityZombie)
                return false;

            if (check(revengeTarget))
            {
                DisplayLog(" Passed the RevengeTarget Tests");
                this.theEntity.SetAttackTarget(revengeTarget, 1200);
                return true;
            }
            Vector3 vector = this.theEntity.position - revengeTarget.position;

            float num = EntityClass.list[this.theEntity.entityClass].SearchArea * 0.5f;
            vector = revengeTarget.position + vector.normalized * (num * 0.5f);
            Vector3 vector2 = UnityEngine.Random.onUnitSphere * num;
            vector.x += vector2.x;
            vector.z += vector2.z;
            Vector3i vector3i = World.worldToBlockPos(vector);
            int height = (int)this.theEntity.world.GetHeight(vector3i.x, vector3i.z);
            if (height > 0)
            {
                vector.y = (float)height;
            }
            this.theEntity.SetInvestigatePosition(vector, 1200);
            this.theEntity.SetAlertTicks(1200);
            this.theEntity.SetRevengeTarget(null);
        }

        if (this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            int EntityID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
            DisplayLog("Leader ID: " + EntityID);
            EntityAlive leader = this.theEntity.world.GetEntity(EntityID) as EntityAlive;
            if (leader)
            {

                DisplayLog(" I have a leader");
                if (leader.GetDamagedTarget() != null)
                {
                    DisplayLog(" Leader's target: " + leader.GetDamagedTarget().EntityName);
                    this.theEntity.SetAttackTarget(leader.GetDamagedTarget(), 1200);
                    return true;
                }
                else
                    DisplayLog("Leader does not have a damaged target");
                CheckSurroundingEntities(leader);
            }
            else
            {
                DisplayLog(" Leader is not set.");
            }

        }
        else
            DisplayLog(" I have no leader.");
        return false;
    }

    public bool CheckSurroundingEntities( EntityAlive leader )
    {
        this.NearbyEntities.Clear();

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(30f, 20f, 30f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {
                if (x.GetAttackTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked.");
                    this.theEntity.SetAttackTarget(x, 1200);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" My leader is a revenger target.");
                    this.theEntity.SetAttackTarget(x, 1200);
                    return true;
                }
            }
        }

        return false;
    }
    protected new bool check(EntityAlive _e)
    {
        if (_e == null)
        {
            DisplayLog("Check is null");
            return false;
        }
        if (_e == this.theEntity)
        {
            DisplayLog("Revenge target is myself.");
            return false;
        }
        if (!_e.IsAlive())
        {
            DisplayLog("Entity is alive.");
            return false;
        }
        Vector3i vector3i = World.worldToBlockPos(_e.position);
        if (!this.theEntity.isWithinHomeDistance(vector3i.x, vector3i.y, vector3i.z))
        {
            DisplayLog("Entity is not within home distance");
            return false;
        }

        return true;
    }
}

