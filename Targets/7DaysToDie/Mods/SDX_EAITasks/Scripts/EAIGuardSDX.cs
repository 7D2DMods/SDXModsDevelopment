using System;
using System.Collections.Generic;
using UnityEngine;

class EAIGuardSDX : EAILook
    {

    float originalView;
    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public bool FetchOrders( )
    {
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Stay))
                return false;

        if ( this.theEntity is EntityAliveSDX )
        {
            EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
            float sqrMagnitude = (temp.GuardPosition - temp.position).sqrMagnitude;
            DisplayLog(" Magnitude from Guard " + temp.GuardPosition + " and Position " + temp.position + " is " + sqrMagnitude);
            if (sqrMagnitude > 1f)
            {
                DisplayLog(" Moving to my guard position ");
                this.theEntity.moveHelper.SetMoveTo(temp.GuardPosition, false);
                return true;
            }
        }

        originalView = this.theEntity.GetMaxViewAngle();
        this.theEntity.SetMaxViewAngle(180f);
        return true;
    }

    public override void Reset()
    {
        this.theEntity.SetLookPosition(Vector3.zero);
        if ( this.theEntity is EntityAliveSDX )
            this.theEntity.SetLookPosition((this.theEntity as EntityAliveSDX).GuardLookPosition);

        // Reset the view angle, and rotate it back to the original look vector.
        this.theEntity.SetMaxViewAngle(this.originalView);
        this.theEntity.RotateTo(this.theEntity.GetLookVector().x, this.theEntity.GetLookVector().y, this.theEntity.GetLookVector().z, 30f, 30f);

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
        return base.Continue();
    }
}

