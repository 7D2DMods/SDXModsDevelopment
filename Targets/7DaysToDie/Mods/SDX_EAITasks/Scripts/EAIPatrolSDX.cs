﻿using GamePath;
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
            Debug.Log( this.GetType() + " : " +this.theEntity.EntityName + ": " + strMessage);
    }

    public bool FetchOrders()
    {
        DisplayLog(" Fetch Orders");
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder"))
        {
            DisplayLog(" Reading Current Order");
            if (this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Patrol)
            {
                DisplayLog(" Current Order is Patrolling.");
                EntityAliveSDX temp = this.theEntity as EntityAliveSDX;
                if (temp)
                {
                    DisplayLog(" Patrol Points: " + temp.PatrolCoordinates.Count);
                    if (temp.PatrolCoordinates.Count > 2)
                        return true;
                }
            }
        }

        DisplayLog(" Fetch Orders failed.");
        return false;
    }

    public void SetPatrolVectors()
    {
        this.PatrolPointsCounter = 0;

        DisplayLog(" Setting Up Patrol Vectors");
        // If this is an entityAliveSDX, check to see if there's any patrol points.
        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if (myEntity)
        {
            DisplayLog(" Patrol Counters: " + myEntity.PatrolCoordinates.Count);
            if (myEntity.PatrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
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
        {
            DisplayLog(" I have an intesgation Position. Starting to Patrol");
            return true;
        }
        DisplayLog(" No Investigation Position");
        return false;
    }

    public override bool Continue()
    {
        // No order and no patrol. Do reverse ( != checks on these, rather than == as it can leave the entity imprecise.
        if (!this.theEntity.Buffs.HasCustomVar("CurrentOrder") || this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Patrol)
        {
            DisplayLog(" Current Order is Not Patrol.");
            return false;
        }
        if (this.lstPatrolPoints.Count <= 0)
        {
            DisplayLog(" Patrol Point Count is too low.");
            return false;
        }
        // If there's an attack target, don't patrol anymore.
        if (this.theEntity.GetAttackTarget() != null)
        {
            DisplayLog(" I have an attack target. No longer patrolling.");
            this.theEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.None, true);
            return false;
        }

        DisplayLog(" Continueing to Patrol");
        return true;
    }

    public override void Update()
    {
        DisplayLog(" Seek Position:" + this.seekPos);
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

            DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + this.lstPatrolPoints.Count );
            DisplayLog(" Vector: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());
        
            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);
            nextCheck = Time.time + this.theEntity.GetMoveSpeed();
            this.theEntity.SetLookPosition(Vector3.forward);
            this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);
        }
    }

}

