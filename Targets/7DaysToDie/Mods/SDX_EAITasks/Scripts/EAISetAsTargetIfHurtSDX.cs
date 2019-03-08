using System;
using System.Collections.Generic;
using UnityEngine;

class EAISetAsTargetIfHurtSDX : EAISetAsTargetIfHurt
{
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log( this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
        if (revengeTarget && revengeTarget != this.theEntity.GetAttackTarget())
        {
            DisplayLog(" I have a revenge Target!");
            return true;
        }
        return false;
    }

}

