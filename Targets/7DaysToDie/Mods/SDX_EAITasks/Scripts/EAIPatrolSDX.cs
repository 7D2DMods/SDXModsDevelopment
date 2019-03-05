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
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder"))
        {
            if (this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
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
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
                this.theEntity.SetInvestigatePosition(this.lstPatrolPoints[PatrolPointsCounter], 1200);
            }
        }
    }

    public override bool CanExecute()
    {
        if (!FetchOrders())
            return false;

        SetPatrolVectors();
        if (this.theEntity.HasInvestigatePosition)
            return true;

        return false;
    }

    public override bool Continue()
    {
        // No order and no patrol. Do reverse ( != checks on these, rather than == as it can leave the entity imprecise.
        if (!this.theEntity.Buffs.HasCustomVar("CurrentOrder") || this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Patrol)
            return false;

        if (this.lstPatrolPoints.Count <= 0)
            return false;

        // If there's an attack target, don't patrol anymore.
        if (this.theEntity.GetAttackTarget() != null)
        {
            this.theEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.None, true);
            return false;
        }
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
                PatrolPointsCounter--;
            else
                PatrolPointsCounter++;

            if (this.lstPatrolPoints.Count > PatrolPointsCounter)
                return;

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);

            nextCheck = Time.time + this.theEntity.GetMoveSpeed();
            this.theEntity.SetLookPosition(Vector3.forward);
            this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);
        }
    }

}


//using GamePath;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//class EAIPatrolSDX : EAIApproachSpot
//{
//    private List<Vector3> lstPatrolPoints = new List<Vector3>();
//    private int PatrolPointsCounter = 0;
//    private bool Retracing = false;

//    public System.Random random = new System.Random();

//    private float nextCheck = 0;
//    public float CheckDelay = 2f;
//    private bool hadPath;

//    private int investigateTicks;

//    private int pathRecalculateTicks;
//    private Vector3 investigatePos;
//    private Vector3 seekPos;

//    bool Lost = false;
//    private int ticker = 0;
//    private bool blDisplayLog = true;

//    public void DisplayLog(String strMessage)
//    {
//        if (blDisplayLog)
//            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
//    }

//    public virtual bool FetchOrders()
//    {
//        if (this.theEntity.Buffs.HasCustomVar("$CurrentOrder"))
//        {
//            if (this.theEntity.Buffs.GetCustomVar("$CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
//            {
//                EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
//                if (temp)
//                {
//                    if (temp.PatrolCoordinates.Count > 2)
//                        return true;
//                }
//            }
//        }

//        return false;
//    }

//    public void SetPatrolVectors()
//    {
//        this.PatrolPointsCounter = 0;

//        // If this is an entityAliveSDX, check to see if there's any patrol points.
//        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
//        if (myEntity)
//        {
//            if (myEntity.PatrolCoordinates.Count > 0)
//            {
//                this.lstPatrolPoints = myEntity.PatrolCoordinates;
//                SetNewDestination(this.lstPatrolPoints[0], 500);
//                PatrolPointsCounter = this.lstPatrolPoints.Count - 1 ;
//            }
//        }
//    }
//    public override void Start()
//    {
//        this.hadPath = false;
//        this.updatePath();
//    }
//    public override bool CanExecute()
//    {
//        if (!FetchOrders())
//            return false;

//        SetPatrolVectors();

//        if (!this.theEntity.HasInvestigatePosition)
//        {
//            return false;
//        }
//        return true;
//    }

//    public override bool Continue()
//    {
//        if (!this.theEntity.Buffs.HasCustomVar("$CurrentOrder") && this.theEntity.Buffs.GetCustomVar("$CurrentOrder") != (float)EntityAliveSDX.Orders.Patrol)
//            return false;

//        float sqrMagnitude = (this.lstPatrolPoints[PatrolPointsCounter] - this.theEntity.position).sqrMagnitude;
//        if (sqrMagnitude <= 4.1f)
//        {
//            //            DisplayLog(" Too far away from desitination: " + sqrMagnitude + " Investigate Position: " + this.investigatePos + " Entity: " + this.theEntity.position);
//            //          this.pathRecalculateTicks = 0;
//            //        SetNewDestination(this.lstPatrolPoints[0], 1000);
//            SetNewDestination(FindNewDestination(), 100);
//            return true;
//        }
//        //else
//        //    SetNewDestination(this.lstPatrolPoints[0], 1000);


//        //PathNavigate navigator = this.theEntity.navigator;
//        //PathEntity path = navigator.getPath();
//        //if (this.hadPath && path == null)
//        //{
//        //    SetNewDestination(FindNewDestination(), 100);
//        //    DisplayLog("No Path");
//        //    return true;
//        //   // return false;
//        //}
//        //if (++this.investigateTicks > 40)
//        //{
//        //    this.investigateTicks = 0;
//        //    if (!this.theEntity.HasInvestigatePosition)
//        //    {
//        //        DisplayLog(" No investigation Position");
//        //        SetNewDestination(FindNewDestination(), 1000);
//        //        return true;
//        //    }
//        //    float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
//        //    if (sqrMagnitude >= 4f)
//        //    {
//        //        DisplayLog(" Too far away from desitination");
//        //        this.pathRecalculateTicks = 0;
//        //        //SetNewDestination(this.lstPatrolPoints[0], 1000);
//        //        return true;
//        //    }

//        //}
//        //float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
//        //if (sqrMagnitude2 <= 1f || (path != null && path.isFinished()))
//        //{
//        //    SetNewDestination(FindNewDestination(), 100);
//        //    //return false;
//        //}
//        return true;

//        //if (! base.Continue())
//        //{
//        //    DisplayLog(" Investigation Spot: " + this.theEntity.InvestigatePosition);
//        //    float sqrMagnitude = (this.lstPatrolPoints[PatrolPointsCounter] - this.theEntity.position + Vector3.down).sqrMagnitude;
//        //    DisplayLog(" Goal: " + this.lstPatrolPoints[PatrolPointsCounter] + " Current: " + this.theEntity.position);
//        //    DisplayLog(" Magnitude Difference: " + sqrMagnitude);
//        //    if (sqrMagnitude <= 20f)
//        //    {
//        //        DisplayLog(" I am within one block of my target. Searching for next point");
//        //        SetNewDestination(FindNewDestination(), 100);
//        //    }
//        //    else
//        //    {
//        //        DisplayLog(" I am off target. Going back to the start.");
//        //        return false;
//        //    }
//        //}

//        ////else
//        ////{
//        ////  
//        ////    PatrolPointsCounter = 0;
//        ////    SetNewDestination(this.lstPatrolPoints[ PatrolPointsCounter] + Vector3.up, 2000);
//        ////}
//        //return true;
//    }

//    public override void Update()
//    {
//        MoveEntity();
//        return;
//        PathEntity path = this.theEntity.navigator.getPath();
//        if (path != null)
//        {
//            this.hadPath = true;
//            this.theEntity.moveHelper.CalcIfUnreachablePos(this.seekPos);
//        }
//        Vector3 lookPosition = this.investigatePos;
//        lookPosition.y += 0.8f;
//        this.theEntity.SetLookPosition(lookPosition);
//        if (--this.pathRecalculateTicks <= 0)
//        {
//            this.updatePath();
//        }
//    }

//    // Token: 0x06002E3B RID: 11835 RVA: 0x00142324 File Offset: 0x00140524
//    private void updatePath()
//    {
//        if (this.theEntity.IsScoutZombie)
//        {
//            AstarManager.Instance.AddLocationLine(this.theEntity.position, this.seekPos, 32);
//        }
//        if (PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
//        {
//            return;
//        }
//        this.pathRecalculateTicks = 40 + this.theEntity.GetRandom().Next(20);
//        PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeedAggro(), true, this);
//    }
//    public Vector3 FindNewDestination()
//    {
//        if (PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
//            Retracing = true;

//        if (PatrolPointsCounter == 0)
//            Retracing = false;

//        if (Retracing)
//            PatrolPointsCounter--;
//        else
//            PatrolPointsCounter++;

//        return this.lstPatrolPoints[PatrolPointsCounter];
//    }
//    public void SetNewDestination(Vector3 position, int ticks )
//    {
//        DisplayLog(" Set New Destination: " + position);
//        this.investigatePos = position ;
//        this.seekPos = this.theEntity.world.FindSupportingBlockPos( position);
//        this.theEntity.SetInvestigatePosition(this.seekPos, ticks);
//    }

//    public virtual void MoveEntity()
//    {
//        if (--this.pathRecalculateTicks <= 0)
//        {

//            float sqrMagnitude = (this.seekPos - this.theEntity.position).sqrMagnitude;
//            DisplayLog(" My Squre Magnitude: " + sqrMagnitude);
//            if (sqrMagnitude <= 4f)

//            // DisplayLog(" I am within one block of my target. I'm no longer lost.");
//            //if (nextCheck < Time.time)
//            {
//                //DisplayLog("MoveEntity()");
//                //if (PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
//                //    Retracing = true;

//                //if (PatrolPointsCounter == 0)
//                //    Retracing = false;

//                //if (Retracing)
//                //    PatrolPointsCounter--;
//                //else
//                //    PatrolPointsCounter++;

//                //this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);

//                DisplayLog("Seek Position: " + this.seekPos);
//                nextCheck = Time.time + this.theEntity.GetMoveSpeed();// + random.Next(5);
//                this.theEntity.SetLookPosition(Vector3.forward);
//                this.theEntity.moveHelper.SetMoveTo(this.seekPos, false);
//                // SetNewDestination(FindNewDestination(), 100);
//            }
//            //else
//            //{
//            //    DisplayLog("Lost");
//            //    PatrolPointsCounter = 0;
//            //    SetNewDestination(this.lstPatrolPoints[0], 1000);
//            //        }
//        }      

//    }
//}

