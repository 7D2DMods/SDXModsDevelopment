using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetIfLeaderAttackedSDX : EAISetAsTargetIfHurt
{
    private List<Entity> NearbyEntities = new List<Entity>();

    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log( this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {

        if (this.theEntity.GetAttackTarget() != null)
            return false;

        if (this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            int EntityID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
            EntityAlive leader = this.theEntity.world.GetEntity(EntityID) as EntityAlive;
            if (leader)
            {
                DisplayLog(" I have a leader: " + EntityID);
                // We have leader, but the leader (player) does not set who its attack or revenge target is.
                // So instead we want to look all around us and check all the entities to see if they are targetting the player. If they are,
                // set them as the AttackTarget for this entity.
                if (!CheckSurroundingEntities(leader))
                    return false;
            }
            else
                DisplayLog(" I do not have a leader.");
        }
  
        return false;
    
    }



    public bool CheckSurroundingEntities(EntityAlive leader)
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
                    DisplayLog(" My leader is being attacked by " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" My leader is being avenged by " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }

                if (x.GetDamagedTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by something that damaged it " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }
            }
        }

        return false;
    }
}

