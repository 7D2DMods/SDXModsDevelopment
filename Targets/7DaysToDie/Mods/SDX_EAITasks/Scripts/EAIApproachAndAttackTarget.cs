using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

class EAIApproachAndAttackTargetSDX : EAIApproachAndAttackTarget
{
    public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || this.theEntity.Jumping)
            return false;

        this.entityTarget = this.theEntity.GetAttackTarget();
        if (this.entityTarget == null)
            return false;

        return true;
    }

}


