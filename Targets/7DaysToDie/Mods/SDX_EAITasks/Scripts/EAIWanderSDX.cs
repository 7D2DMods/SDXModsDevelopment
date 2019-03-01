using System;
using System.Collections.Generic;
using UnityEngine;

class EAIWanderSDX : EAIWander
    {
    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders( )
    {
        if (this.theEntity.Buffs.HasCustomVar("$CurrentOrder"))
        {
            if (this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Stay)
            {
                // Order to stay
                return false;
            }
        }
    
        return true;
    }
    public override bool CanExecute()
    {
        return FetchOrders()  && base.CanExecute();
    }

    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        if (this.theEntity.moveHelper.BlockedTime <= 1f)
            return false;


        return base.Continue();
    }
}

