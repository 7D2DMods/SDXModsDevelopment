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
    EntityAliveSDX entityAlive;
    private float PatrolSpeed = 2f;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        EntityClass entityClass = EntityClass.list[_theEntity.entityClass];
        if (entityClass.Properties.Values.ContainsKey("PatrolSpeed"))
            this.PatrolSpeed = float.Parse(entityClass.Properties.Values["PatrolSpeed"]);
    }
    public bool FetchOrders()
    {
        DisplayLog(" Fetch Orders");

        entityAlive = this.theEntity as EntityAliveSDX;
        if (entityAlive)
        {
            DisplayLog(" Patrol Points: " + entityAlive.PatrolCoordinates.Count);
            if (entityAlive.PatrolCoordinates.Count > 2)
                return true;
        }

        return false;
    }

    public void SetPatrolVectors()
    {
        // this.PatrolPointsCounter = 0;


        DisplayLog(" Setting Up Patrol Vectors");
        // If this is an entityAliveSDX, check to see if there's any patrol points.
        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if (myEntity)
        {

            // If we already have patrol points, and they are the same as we have, don't reset.
            if (this.lstPatrolPoints.Count > 0 && this.lstPatrolPoints == myEntity.PatrolCoordinates )
                return;

            DisplayLog(" Patrol Counters: " + myEntity.PatrolCoordinates.Count);
            if (myEntity.PatrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
                this.lstPatrolPoints = myEntity.PatrolCoordinates;
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
            }
        }
    }

    public override bool CanExecute()
    {

        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && this.theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Patrol)
            return false;

        // If there's an attack target, don't patrol anymore.
        if (this.theEntity.GetAttackTarget() != null && this.theEntity.GetAttackTarget().IsAlive())
        {
            DisplayLog(" I have an attack target. No longer patrolling.");
            return false;
        }

        if (!FetchOrders())
            return false;

        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;

        SetPatrolVectors();
        this.theEntity.SetInvestigatePosition(this.lstPatrolPoints[PatrolPointsCounter], 1200);
        if (this.theEntity.HasInvestigatePosition)
        {
            DisplayLog(" I have an intesgation Position. Starting to Patrol");
            return true;
        }
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
            //  this.theEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.None, true);
            return false;
        }

        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;

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

            DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + this.lstPatrolPoints.Count);
            DisplayLog(" Vector: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);
            nextCheck = Time.time + this.PatrolSpeed;// this.theEntity.GetMoveSpeed();

            this.theEntity.SetLookPosition(Vector3.forward);
            this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);
        }
    }

}

