using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

class EAIApproachAndAttackTargetSDX : EAIApproachAndAttackTarget
{

    List<String> lstIncentives = new List<String>();

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || this.theEntity.Jumping)
            return false;

        
        this.entityTarget = this.theEntity.GetAttackTarget();
        if (this.entityTarget == null)
            return false;


        DisplayLog("My Current Target is: " + this.theEntity.ToString());

        // This checks against param2, which is incentive NOT to attack
        if ( CheckIncentive( entityTarget ))
        {
            this.theEntity.SetAttackTarget(null, 0);
            return false;
        }
        return true;
    }

    // Allow params to be a comma-delimited list of various incentives, such as item name, buff, or cvar.
    public override void SetParams2(string _par1)
    {
        string[] array = _par1.Split(new char[]
        {
                ','
        });
        for (int i = 0; i < array.Length; i++)
        {
            if (this.lstIncentives.Contains(array[i].ToString()))
                continue;
            this.lstIncentives.Add(array[i].ToString());
        }
    }

    // Checks a list of buffs to see if there's an incentive for it to execute.
    public virtual bool CheckIncentive(EntityAlive entity)
    {
        bool result = false;
        foreach (String strIncentive in this.lstIncentives)
        {
            // Check if the entity that is looking at us has the right buff for us to follow.
            if (entity.Buffs.HasBuff(strIncentive))
                result = true;

            // Check if there's a cvar for that incentive, such as $Mother or $Leader.
            if (this.theEntity.Buffs.HasCustomVar(strIncentive))
            {
                if (this.theEntity.Buffs.GetCustomVar(strIncentive) == entity.entityId)
                    result = true;
            }

            // Then we check if the control mechanism is an item being held.
            if (entity.inventory.holdingItem.Name == strIncentive)
                result = true;

            // if we are true here, it means we found a match to our entity.
            if (result)
                break;
        }

        return result;
    }

}


