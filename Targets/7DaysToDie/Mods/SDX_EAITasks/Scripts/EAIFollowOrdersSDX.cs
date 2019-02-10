using UnityEngine;
using System.Reflection;
using System;

class EAIFollowOrdersSDX : EAIApproachAndAttackTarget
{
    public void DisplayLog(string strMessage)
    {
        Debug.Log(this.GetType().Name + ": " + strMessage);
    }

    public override bool CanExecute()
    {
       // DisplayLog(MethodBase.GetCurrentMethod().Name);

        // Check if we have a master
        if (this.theEntity.otherEntitySDX != null)
        {
           // DisplayLog(" I have a master: " + this.theEntity.otherEntitySDX.EntityName);

            WorldRayHitInfo hitInfo = (this.theEntity.otherEntitySDX as EntityPlayerLocal).HitInfo;
            if (hitInfo != null && hitInfo.bHitValid && hitInfo.transform)
            {
             //   DisplayLog(" Is the Attack Valid: " + this.theEntity.otherEntitySDX.IsAttackValid());
               // DisplayLog(" My master is looking at something.");
                Transform hitRootTransform;
                if ((hitRootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform)) != null)
                {
                    EntityAlive myTarget = hitRootTransform.GetComponent<EntityAlive>();
                    if ( myTarget != null)
                    {
                 //       DisplayLog("My master is looking at an EntityAlive");
                        if (myTarget.GetAttackTarget() == this.theEntity.otherEntitySDX || myTarget.GetRevengeTarget() == this.theEntity.otherEntitySDX)
                        {
                            this.theEntity.SetAttackTarget(myTarget, 60);
                            base.entityTarget = myTarget;
                   //         DisplayLog(" My master has an Attack Target. I now share the vengence against : " + this.theEntity.GetAttackTarget().name);
                            return true;
                        }
                        if (myTarget = base.theEntity)
                        {
                            DisplayLog("master is looking at me.");
                        }
                    }
                   
                }
            }
        }
        return false;
    }
}


