using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
    {
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders( )
    {
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Wander))
                return false;
        return true;
    }
    public override bool CanExecute()
    {
        if (!FetchOrders())
            return false;

        return base.CanExecute();
    }

    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        // if an entity gets 'stuck' on a block, it just starts attacking it. Kind of aggressive.
        if (this.theEntity.moveHelper.BlockedTime <= 1f)
            return false;


        return base.Continue();
    }
}

