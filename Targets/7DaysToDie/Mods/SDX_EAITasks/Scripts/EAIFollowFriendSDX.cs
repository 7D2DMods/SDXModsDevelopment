using UnityEngine;
using System.Reflection;
using System;

class EAIFollowFriendSDX : EAIApproachAndFollowTargetSDX
{

    public override bool CanExecute()
    {
       DisplayLog(MethodBase.GetCurrentMethod().Name);

        // Check if we have a master
        if (this.theEntity.otherEntitySDX != null)
        {

        }
        return false;
    }
}


