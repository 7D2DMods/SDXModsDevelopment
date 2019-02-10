using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

class EAIApproachAndAttackTargetSDX : EAIApproachAndAttackTarget
{
    private String strControlMechanism = "";
    private List<Entity> NearbyEntities = new List<Entity>();

    public void DisplayLog(string strMessage)
    {
        Debug.Log(this.GetType().Name + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        bool result = base.CanExecute();

        ConfigureTargetEntity();

        // Check if we have a master
        if (this.theEntity.otherEntitySDX != null && base.entityTarget != null)
        {
            // If its dead, don't attack
            if (base.entityTarget.IsDead())
                return false;

            // If it's you, don't attack
            if (base.entityTarget == this.theEntity)
            {
                base.entityTarget = null;
                this.theEntity.SetAttackTarget(null, 0);

                return false;
            }

            // Check if the target entity is the master.
            if (this.theEntity.otherEntitySDX == base.entityTarget)
            {
                // Don't attack your master!
                this.theEntity.SetAttackTarget(null, 0);
                return false;
            }
            else
            {
                this.theEntity.SetAttackTarget(base.entityTarget, 0);
            }

        }
        DisplayLog(" Result: " + result);
        return result;
    }

    public void ConfigureTargetEntity()
    {
        this.NearbyEntities.Clear();

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(20f, 20f, 20f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {
                // Check if the there's an entity in our area that is allowed to control us.

                // first, we check if we are controlled via an activate buff.
                if (x.Buffs.HasBuff(this.strControlMechanism))
                    this.theEntity.otherEntitySDX = x;

                // Then we check if the control mechanism is an item being held.
                if (x.inventory.holdingItem.Name == this.strControlMechanism)
                    this.theEntity.otherEntitySDX = x;

                // So we have a master entity. Let's decide what the master wants us to do.
                if (this.theEntity.otherEntitySDX != null)
                {
                    // if the entity is our master, don't set it as an attack target.
                    if (x == this.theEntity.otherEntitySDX)
                        continue;
                    base.entityTarget = x;
                    return;
                    
                }
            }
        }
    }

    public override void SetParams2(string _par2)
    {
        this.strControlMechanism = _par2;
    }
}


