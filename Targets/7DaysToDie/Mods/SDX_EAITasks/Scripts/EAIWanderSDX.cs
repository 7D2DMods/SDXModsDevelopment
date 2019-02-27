using System;
    class EAIWanderSDX : EAIWander
    {

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

