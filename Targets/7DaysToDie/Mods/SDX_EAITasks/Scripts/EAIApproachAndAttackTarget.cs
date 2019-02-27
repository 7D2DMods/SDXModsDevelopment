using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

class EAIApproachAndAttackTargetSDX : EAIApproachAndFollowTargetSDX
{
    public override bool CanExecute()
    {
        bool result = base.CanExecute();

        
        if (ConfigureTargetEntity())
        {
            if (entityTarget != null)
            {
                // Check if our leader has an attack target.
                EntityAlive target = entityTarget.GetAttackTarget();
                if (target == null)
                    return false;

            }
        }
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

}


