using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{

    List<String> lstIncentives = new List<String>();
    private List<Entity> NearbyEntities = new List<Entity>();
    float distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);

    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;


    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log( this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }

    // Allow params to be a comma-delimited list of various incentives, such as item name, buff, or cvar.
    public override void SetParams1(string _par1)
    {
        string[] array = _par1.Split(new char[]
        {
                ','
        });
        for (int i = 0; i < array.Length; i++)
        {
            if (this.lstIncentives.Contains(array[i].ToString()))
                continue;
            this.lstIncentives.Add(array[i].ToString());
        }
    }

    // Checks a list of buffs to see if there's an incentive for it to execute.
    public virtual bool CheckIncentive(EntityAlive entity)
    {
        bool result = false;
        foreach (String strIncentive in this.lstIncentives)
        {
            // Check if the entity that is looking at us has the right buff for us to follow.
            if (entity.Buffs.HasBuff(strIncentive))
                result = true;

            // Check if there's a cvar for that incentive, such as $Mother or $Leader.
            if (this.theEntity.Buffs.HasCustomVar(strIncentive))
            {
                if (this.theEntity.Buffs.GetCustomVar(strIncentive) == entity.entityId)
                    result = true;
            }

            // Then we check if the control mechanism is an item being held.
            if (entity.inventory.holdingItem.Name == strIncentive)
                result = true;

            // if we are true here, it means we found a match to our entity.
            if (result)
                break;
        }

        if (result)
            this.entityTarget = entity;

        return result;
    }

    public virtual bool ConfigureTargetEntity()
    {
        this.NearbyEntities.Clear();

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(30f, 20f, 30f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {
                // Check the entity against the incentives
                if (CheckIncentive(x))
                    return true;
            }
        }

        this.entityTarget = null;

    
        return false;
    }

    public override bool CanExecute()
    {
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Stay))
            return false;

        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || this.theEntity.Jumping)
            return false;

        if (this.theEntity.GetAttackTarget() != null) // If it has an attack target, break
        {
            DisplayLog(" I have an attack Target: " + this.theEntity.GetAttackTarget());
            return false;
        }
        if (this.theEntity.GetRevengeTarget() != null) // If something attacks you, break
        {
            DisplayLog(" I have a revenge Target: " + this.theEntity.GetRevengeTarget());
            return false;
        }

        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Loot)
        {
            DisplayLog(" I am looting. Not following the leader.");
            return false;
        }
        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;
        if (!this.theEntity.Buffs.HasCustomVar("Leader"))
            return false;
        // Change the distance allowed each time. This will give it more of a variety in how close it can get to you.
        distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        return ConfigureTargetEntity();

    }

    public override bool Continue()
    {
        if (this.theEntity.Buffs.HasCustomVar("CurrentOrder") && (this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Stay))
            return false;

        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
            return false;

        if (pathCounter == 0) // briefly pause if you are at the end of the path to let other tasks run
            return false;

        if (this.theEntity.GetAttackTarget() != null) // If it has an attack target, break
        {
            DisplayLog(" I have an attack Target: " + this.theEntity.GetAttackTarget());
            return false;
        }
        if (this.theEntity.GetRevengeTarget() != null) // If something attacks you, break
        {
            DisplayLog(" I have a revenge Target: " + this.theEntity.GetRevengeTarget());
            return false;
        }

        // if The entity is busy, don't continue patrolling.
        bool isBusy = false;
        if (this.theEntity.emodel.avatarController.TryGetBool("IsBusy", out isBusy))
            if (isBusy)
                return false;
        if (this.theEntity.Buffs.HasCustomVar("Leader"))
        {
            if ((int)this.theEntity.Buffs.GetCustomVar("Leader") == 0)
                return false;
        }
        else
            return false;
        return ConfigureTargetEntity();
        
    }
    public override void Update()
    {
        Vector3 position = Vector3.zero;
        float targetXZDistanceSq = 0f;

        // No entity, so no need to do anything.
        if (this.entityTarget == null)
            return;

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());

        // Find the location of the entity, and figure out where it's at.
        position = this.entityTarget.position;
        targetXZDistanceSq = base.GetTargetXZDistanceSq(6);

        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if (myEntity)
        {
            if (this.theEntity.Buffs.HasCustomVar("CurrentOrder"))
            {
                if (this.theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.SetPatrolPoint)
                {
                    // Make them a lot closer to you when they are following you.
                    this.distanceToEntity = 1f;

                    myEntity.UpdatePatrolPoints(this.theEntity.world.FindSupportingBlockPos( this.entityTarget.position));
                }
            }
        }
        Vector3 a = position - this.entityTargetPos;
        if (a.sqrMagnitude < 1f)
            this.entityTargetVel = this.entityTargetVel * 0.7f + a * 0.3f;

        this.entityTargetPos = position;

        this.theEntity.moveHelper.CalcIfUnreachablePos(position);

        float num2 = distanceToEntity * distanceToEntity;

        float num3 = position.y - this.theEntity.position.y;
        float num4 = Utils.FastAbs(num3);
        bool flag = targetXZDistanceSq <= num2 && num4 < 1f;

        // num is used to determine how close and comfortable the entity approaches you, so let's make sure they respect some personal space
        if (distanceToEntity < 1)
            distanceToEntity = 3;

        if (!flag)
        {
            if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
            {
                PathEntity path = this.theEntity.navigator.getPath();
                if (path != null && path.NodeCountRemaining() <= 2)
                    this.pathCounter = 0;
            }
            if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
            {
                this.pathCounter = 6 + this.theEntity.GetRandom().Next(10);
                DisplayLog(" Distance: " + distanceToEntity);
                Vector3 moveToLocation = this.GetMoveToLocation(distanceToEntity);
                PathFinderThread.Instance.FindPath(this.theEntity, moveToLocation, this.theEntity.GetMoveSpeedAggro(), true, this);
            }
        }
        if (this.theEntity.Climbing)
        {
            return;
        }
        if (!flag)
        {
            if (this.theEntity.navigator.noPathAndNotPlanningOne() && num3 < 2.1f)
            {
                DisplayLog(" Distance2: " + distanceToEntity);
                Vector3 moveToLocation2 = this.GetMoveToLocation(distanceToEntity);
                this.theEntity.moveHelper.SetMoveTo(moveToLocation2, true);
            }
        }
        else
        {
            this.theEntity.navigator.clearPath();
            this.theEntity.moveHelper.Stop();
            this.pathCounter = 0;
        }
    }
}
