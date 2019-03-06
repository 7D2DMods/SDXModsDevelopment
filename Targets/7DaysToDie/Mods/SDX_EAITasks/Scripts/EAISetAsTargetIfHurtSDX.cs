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

        bool result = base.CanExecute();
        
        // If we can't excute, according to the rules of the base class, check to see if there's a leader buff
    //    if ( !result )
        {
            if (this.theEntity.Buffs.HasCustomVar("Leader"))
            {
                int EntityID = (int)this.theEntity.Buffs.GetCustomVar("Leader");
                EntityAlive leader = this.theEntity.world.GetEntity(EntityID) as EntityAlive;
                if (leader)
                {
                    if (leader.GetAttackTarget() != null)
                    {
                        DisplayLog(" Setting My leader's Attack target to mine.");
                        this.theEntity.SetAttackTarget(leader.GetAttackTarget(), 1200);
                        return false;
                    }

                    if (leader.GetRevengeTarget() != null)
                    {
                        DisplayLog(" Setting My leader's Revenge target to mine.");
                        this.theEntity.SetAttackTarget(leader.GetRevengeTarget(), 1200);
                        return false;
                    }
                    if (leader.GetDamagedTarget() != null)
                    {
                        DisplayLog(" Setting My leader's Damaged target to mine.");
                        this.theEntity.SetAttackTarget(leader.GetDamagedTarget(), 1200);
                        return false;
                    }
                }
                else
                    DisplayLog("My leader is not alive.");
            }
            else
                DisplayLog(" I do not have a cvar fo rleader");
        }
   
    
        return result;
    }
 
}

