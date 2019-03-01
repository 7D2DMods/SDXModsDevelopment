using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;
class EAIPatrolSDX : EAIApproachSpot
{
    private List<Vector3> lstPatrolPoints = new List<Vector3>();
    private int PatrolPointsCounter = 0;
    private bool Retracing = false;

    // Token: 0x04002293 RID: 8851
    private Vector3 investigatePos;

    // Token: 0x04002294 RID: 8852
    private Vector3 seekPos;
    private bool hadPath;

    // Token: 0x04002296 RID: 8854
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
                if ( temp )
                {
                    if (temp.PatrolCoordinates.Count > 0)
                        return true;
                }
            }
        }

        return false;
    }

    public void SetPatrolVectors()
    {
        this.lstPatrolPoints.Clear();
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

        if (!this.theEntity.HasInvestigatePosition)
        {
            return false;
        }
        if (this.theEntity.IsSleeping)
        {
            return false;
        }

        

        this.investigatePos = this.theEntity.InvestigatePosition;
        this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);

        DisplayLog("Seeking Patrol position: " + this.investigatePos.ToString() );

        return true;

        
    }

    public override bool Continue()
    {
        if (!FetchOrders())
            return false;

        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            return false;
        }
        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
            {
                return false;
            }
            float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            if (sqrMagnitude >= 4f)
            {
                return false;
            }
        }
        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 1f || (path != null && path.isFinished()))
        {
            DisplayLog(" Counter: " + PatrolPointsCounter + "  Total List: " + this.lstPatrolPoints.Count);
            if ( PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
                Retracing = true;

            if (PatrolPointsCounter == 0)
                Retracing = false;

            if (Retracing)
                PatrolPointsCounter--;
            else
                PatrolPointsCounter++;

            if (this.lstPatrolPoints[PatrolPointsCounter] == null)
                return false;

           
            this.theEntity.SetInvestigatePosition( this.lstPatrolPoints[PatrolPointsCounter], 400);
            this.investigatePos = this.theEntity.InvestigatePosition;
            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
            return true;
        }
        return true;

      
    }

    public override void Update()
    {
        DisplayLog(" Moving to: " + this.seekPos);
        this.theEntity.MoveEntityHeaded(this.seekPos, true );
    }


}

