using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;
class EAIPatrolSDX : EAIApproachSpot
{
    private List<Vector3> lstPatrolPoints = new List<Vector3>();
    private int PatrolPointsCounter = 0;

    private float nextCheck = 0;
    private Vector3 seekPos;
    
    private EntityAliveSDX entityAliveSDX;

    // Controls the delay in between movements.
    private float PatrolSpeed = 2f;

    private bool blDisplayLog = false;
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

        entityAliveSDX = (_theEntity as EntityAliveSDX);
    }
    public bool FetchOrders()
    {
        DisplayLog(" Fetch Orders");
    
        if (entityAliveSDX)
        {
            DisplayLog(" Patrol Points: " + entityAliveSDX.PatrolCoordinates.Count);
            if (entityAliveSDX.PatrolCoordinates.Count > 2)
                return true;
        }

        return false;
    }

    public void SetPatrolVectors()
    {
        // this.PatrolPointsCounter = 0;
        DisplayLog(" Setting Up Patrol Vectors");
        if (entityAliveSDX)
        {

            // If we already have patrol points, and they are the same as we have, don't reset.
            if (this.lstPatrolPoints.Count > 0 && this.lstPatrolPoints == entityAliveSDX.PatrolCoordinates )
                return;

            DisplayLog(" Patrol Counters: " + entityAliveSDX.PatrolCoordinates.Count);
            if (entityAliveSDX.PatrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
                this.lstPatrolPoints = entityAliveSDX.PatrolCoordinates;
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
            }
        }
    }

    public override bool CanExecute()
    {
        DisplayLog("CanExecute() Start");
        bool result = false;
        if (entityAliveSDX)
        {
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Patrol);
            DisplayLog("CanExecute() Follow Task? " + result);
        }

        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return true;

        if (!FetchOrders())
            result = false;

        SetPatrolVectors();

        this.theEntity.SetInvestigatePosition(this.lstPatrolPoints[PatrolPointsCounter], 1200);
        if (this.theEntity.HasInvestigatePosition)
        {
            DisplayLog(" I have an intesgation Position. Starting to Patrol");
            this.theEntity.emodel.avatarController.SetTrigger("IsPatrolling");
            result = true;
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }

    public override bool Continue()
    {
        // No order and no patrol. Do reverse ( != checks on these, rather than == as it can leave the entity imprecise.
        bool result = false;
        if (entityAliveSDX)
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Patrol);

        if (this.lstPatrolPoints.Count <= 0)
        {
            DisplayLog(" Patrol Point Count is too low.");
            result  =false;
        }
   
        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;

        DisplayLog(" Continueing to Patrol");
        return result;
    }

    public override void Update()
    {
        DisplayLog(" Seek Position:" + this.seekPos);
        if (nextCheck < Time.time)
        {
            this.PatrolPointsCounter = (this.PatrolPointsCounter + 1) % this.lstPatrolPoints.Count;

            DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + this.lstPatrolPoints.Count);
            DisplayLog(" Vector: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);
            nextCheck = Time.time + this.PatrolSpeed;// this.theEntity.GetMoveSpeed();

            this.theEntity.SetLookPosition(this.seekPos);
            this.theEntity.RotateTo( this.seekPos.x, this.seekPos.y, this.seekPos.z,  30f, 30f);

            this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);
        }
    }

}

