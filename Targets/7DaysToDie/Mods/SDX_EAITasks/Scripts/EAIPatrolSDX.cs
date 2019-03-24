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
    private int pathRecalculateTicks;

    private EntityAliveSDX entityAliveSDX;

    // Controls the delay in between movements.
    private float PatrolSpeed = 2f;

    private bool blDisplayLog = true;
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
            DisplayLog(" Patrol Counters: " + entityAliveSDX.PatrolCoordinates.Count);
            if (entityAliveSDX.PatrolCoordinates.Count > 0)
            {
                DisplayLog(" Setting up Patrol Coordinates");
                this.lstPatrolPoints = entityAliveSDX.PatrolCoordinates;
                PatrolPointsCounter = this.lstPatrolPoints.Count - 1;
                this.seekPos = this.lstPatrolPoints[PatrolPointsCounter];
            }
            else
            {
                PatrolPointsCounter = 0;
                this.lstPatrolPoints.Clear();
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
            if (result == false)
                return false;
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

    bool blReverse = true;
    public override void Update()
    {
        //DisplayLog(" Seek Position:" + this.seekPos);
        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        Debug.Log(" Magnitude:" + sqrMagnitude2);
        if (sqrMagnitude2 <= 2f)
        {
        //if (nextCheck < Time.time)
       // {
            if (this.PatrolPointsCounter == this.lstPatrolPoints.Count - 1)
                blReverse = true;

            if (this.PatrolPointsCounter == 0)
                blReverse = false;

            if (blReverse)
                this.PatrolPointsCounter--;
            else
                this.PatrolPointsCounter++;
            //this.PatrolPointsCounter = (this.PatrolPointsCounter + 1) % this.lstPatrolPoints.Count;

            
            DisplayLog(" Patrol Points Counter: " + PatrolPointsCounter + " Patrol Points Count: " + this.lstPatrolPoints.Count);
            DisplayLog(" Vector: " + this.lstPatrolPoints[PatrolPointsCounter].ToString());

            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.lstPatrolPoints[PatrolPointsCounter]);

            nextCheck = Time.time + this.PatrolSpeed;// this.theEntity.GetMoveSpeed();

            //this.theEntity.RotateTo( this.seekPos.x, this.seekPos.y + 2, this.seekPos.z,  30f, 30f);
            this.theEntity.SetLookPosition( this.seekPos);

            this.theEntity.moveHelper.SetMoveTo(this.lstPatrolPoints[PatrolPointsCounter], false);
            updatePath();
        }
    }

    public override void updatePath()
    {
        if (this.theEntity.IsScoutZombie)
        {
            AstarManager.Instance.AddLocationLine(this.theEntity.position, this.seekPos, 32);
        }
        if (GamePath.PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
        {
            return;
        }
        this.pathRecalculateTicks = 40 + this.theEntity.GetRandom().Next(20);
        GamePath.PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeed(), true, this);
    }
}

