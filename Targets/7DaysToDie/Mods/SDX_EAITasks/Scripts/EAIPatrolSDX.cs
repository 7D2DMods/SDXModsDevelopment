using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;
class EAIPatrolSDX : EAIApproachSpot
{
    private List<Vector3> lstPatrolPoints = new List<Vector3>();
    private int PatrolPointsCounter = 0;
    private bool Retracing = false;

    private float nextCheck = 0;
    public float CheckDelay = 2f;

    private Vector3 investigatePos;
    private Vector3 seekPos;
    private bool hadPath;
    private int investigateTicks;
    private bool blDisplayLog = true;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }

    public bool FetchOrders()
    {
        if (this.theEntity.Buffs.HasCustomVar("$CurrentOrder"))
        {
            if (this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
            {
                EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
                if (temp)
                {
                    if (temp.PatrolCoordinates.Count > 2)
                        return true;
                }
            }
        }

        return false;
    }

    public void SetPatrolVectors()
    {
        this.PatrolPointsCounter = 0;

        // If this is an entityAliveSDX, check to see if there's any patrol points.
        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if (myEntity)
        {
            if (myEntity.PatrolCoordinates.Count > 0)
            {
                this.lstPatrolPoints = myEntity.PatrolCoordinates;
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1 ;
                this.theEntity.SetInvestigatePosition(this.lstPatrolPoints[ PatrolPointsCounter], 1200);
            }
        }
    }

    public override bool CanExecute()
    {
        if (!FetchOrders())
            return false;

        SetPatrolVectors();
        return true;
    }

    public override bool Continue()
    {
        if (this.theEntity.Buffs.HasCustomVar("$CurrentOrder") && this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
                return true;
        return false;
    }

    public override void Update()
    {
        if (nextCheck < Time.time)
        {
            if (PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
                Retracing = true;

            if (PatrolPointsCounter == 0)
                Retracing = false;

            if (Retracing)
                PatrolPointsCounter--;
            else
                PatrolPointsCounter++;

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);

            nextCheck = Time.time + this.theEntity.GetMoveSpeed();
            this.theEntity.SetLookPosition(Vector3.forward);
            this.theEntity.moveHelper.SetMoveTo(this.seekPos , false);
        }
    }
}

