using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetNearestEnemySDX : EAISetAsTargetIfHurt
{
    private List<Entity> NearbyEntities = new List<Entity>();

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {

        if (this.theEntity.GetAttackTarget() != null)
            return false;

        if (this.theEntity.GetRevengeTarget() != null)
            return false;
        return CheckSurroundingEntities(this.theEntity);
    }


    public bool CheckFactionForEnemy(EntityAlive Entity)
    {
        FactionManager.Relationship myRelationship = FactionManager.Instance.GetRelationshipTier(this.theEntity, Entity);
        if (myRelationship == FactionManager.Relationship.Hate)
        {
            DisplayLog(" I hate this entity: " + Entity.ToString());
            return true;
        }
        else
            DisplayLog(" My relationship with this " + Entity.ToString() + " is: " + myRelationship.ToString());
        return false;
    }


    public override bool Continue()
    {
        if (this.theEntity.GetAttackTarget() != null)
            return false;

        if (this.theEntity.GetRevengeTarget() != null)
            return false;

        if (CheckSurroundingEntities(this.theEntity))
            return false;

        return base.Continue();
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
                if (!x.IsAlive())
                    continue;

                if (CheckFactionForEnemy(x))
                {
                    DisplayLog(" I have an enemy in range: " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }
                if (x.GetAttackTarget() == leader)
                {
                    DisplayLog(" I am being targetted by " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" I am being avenged by " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }

                if (x.GetDamagedTarget() == leader)
                {
                    DisplayLog(" An entity has damaged me " + x.ToString());
                    this.theEntity.SetRevengeTarget(x);
                    return true;
                }
            }
        }

        return false;
    }
}

