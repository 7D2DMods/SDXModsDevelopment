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
            DisplayLog("Entity has CurrentOrder CVAR");
            if (this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
            {
                DisplayLog("Current Order is Patrol");
                EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
                if (temp)
                {
                    DisplayLog("Entity is EntityAliveSDX");
                    DisplayLog(" Patrol Points: " + temp.PatrolCoordinates.Count);
                    if (temp.PatrolCoordinates.Count > 0)
                        return true;

                    DisplayLog("Patrol Coordinates count is not greater than 0. No Coordinates");
                }
            }
            else
            {
                DisplayLog("Current Order is not Patrol");
                DisplayLog("Current Order: " + this.theEntity.Buffs.GetCustomVar("$CurrentOrder"));
            }
        }

        return false;
    }

    public void SetPatrolVectors()
    {
       // this.lstPatrolPoints.Clear();
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
        DisplayLog("Can Execute? " + (this.theEntity as EntityAliveSDX).PatrolCoordinates.Count);
        if (!FetchOrders())
            return false;

        DisplayLog("Before Set Patrol " + (this.theEntity as EntityAliveSDX).PatrolCoordinates.Count);
        SetPatrolVectors();
        DisplayLog("After Set Patrol " + (this.theEntity as EntityAliveSDX).PatrolCoordinates.Count);
        //this.investigatePos = this.theEntity.InvestigatePosition;
        //this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);

        //DisplayLog("Seeking Patrol position: " + this.investigatePos.ToString() );

        return true;

        
    }

    public override bool Continue()
    {

        if (this.theEntity.Buffs.HasCustomVar("$CurrentOrder"))
            if (this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
                return true;
        return false;

                //if (!FetchOrders())
                //{
                //    DisplayLog(" FetchOrders is false");
                //    return false;
                //}
                //PathNavigate navigator = this.theEntity.navigator;
                //PathEntity path = navigator.getPath();
                //if (this.hadPath && path == null)
                //{
                //    DisplayLog("No Path");
                //    return false;
                //}
                //if (++this.investigateTicks > 40)
                //{
                //    this.investigateTicks = 0;
                //    if (!this.theEntity.HasInvestigatePosition)
                //    {
                //        DisplayLog("No investigation Position");
                //        return false;
                //    }
                //    float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
                //    if (sqrMagnitude >= 4f)
                //    {
                //        DisplayLog("magnitude 4f");
                //        return false;
                //    }
                //}
                //float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
                //if ( (path != null && path.isFinished()))
                //{
                //    DisplayLog(" Counter: " + PatrolPointsCounter + "  Total List: " + this.lstPatrolPoints.Count);
                //    if ( PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
                //        Retracing = true;

                //    if (PatrolPointsCounter == 0)
                //        Retracing = false;

                //    if (Retracing)
                //        PatrolPointsCounter--;
                //    else
                //        PatrolPointsCounter++;

                //    if (this.lstPatrolPoints[PatrolPointsCounter] == null)
                //    {
                //        DisplayLog("Patrol Point is null");
                //        return false;
                //    }

                //    DisplayLog("Heading to: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());
                //    this.theEntity.SetInvestigatePosition( this.lstPatrolPoints[PatrolPointsCounter], 400);
                //    this.investigatePos = this.theEntity.InvestigatePosition;
                //    this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
                //    return true;
                //}
                return true;

      
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
            {
                PatrolPointsCounter--;
                if (this.lstPatrolPoints[PatrolPointsCounter - 1] != null)
                    this.theEntity.SetLookPosition(this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter - 1]));
            }
            else
            {
                PatrolPointsCounter++;
                if (this.lstPatrolPoints[PatrolPointsCounter + 1] != null)
                    this.theEntity.SetLookPosition(this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter + 1]));
            }

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);
            nextCheck = Time.time + this.theEntity.GetMoveSpeed();
            this.theEntity.moveHelper.SetMoveTo(this.seekPos, true);
        }
    }
}

