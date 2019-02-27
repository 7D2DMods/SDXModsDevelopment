using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{

    private List<Entity> NearbyEntities = new List<Entity>();
    float distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);

    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;

    private String strControlMechanism = "";

    //public override void Init(EntityAlive _theEntity)
    //{
    //    base.Init(_theEntity);
    //    this.MutexBits = 3;
    //    this.executeDelay = 0.1f;
    //}

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }
    public override void SetParams1(string _par1)
    {
        this.strControlMechanism = _par1;
    }

    public virtual void ConfigureTargetEntity()
    {
        this.NearbyEntities.Clear();
        this.theEntity.otherEntitySDX = null;

        // Search in the bounds are to try to find the most appealing entity to follow.
        Bounds bb = new Bounds(this.theEntity.position, new Vector3(20f, 20f, 20f));
        this.theEntity.world.GetEntitiesInBounds(typeof(EntityAlive), bb, this.NearbyEntities);
        for (int i = this.NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAlive x = (EntityAlive)this.NearbyEntities[i];
            if (x != this.theEntity)
            {
                // Check if the there's an entity in our area that is allowed to control us.

                // Check if the control mechanism is a cvar
                if (this.theEntity.Buffs.HasCustomVar(this.strControlMechanism))
                {
                    DisplayLog(" I have a cvar for an incentive:" + this.strControlMechanism );
                    if (this.theEntity.Buffs.GetCustomVar(this.strControlMechanism) == x.entityId)
                    {
                        Debug.Log(" My CVAR value is: " + x.entityId);
                        this.theEntity.otherEntitySDX = x;
                    }
                }
                // first, we check if we are controlled via an activate buff.
                if (x.Buffs.HasBuff(this.strControlMechanism))
                    this.theEntity.otherEntitySDX = x;

                // Then we check if the control mechanism is an item being held.
                if (x.inventory.holdingItem.Name == this.strControlMechanism)
                    this.theEntity.otherEntitySDX = x;

                // If we do have a master entity, and it's still in range, then set it as the target.
                if (this.theEntity.otherEntitySDX != null)
                {
                    base.entityTarget = this.theEntity.otherEntitySDX;
                    return;

                }
            }
        }

    }

    public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || this.theEntity.Jumping)
            return false;

        if (this.theEntity.Buffs.HasCustomVar(this.strControlMechanism) && this.theEntity.Buffs.GetCustomVar(this.strControlMechanism) == 0)
            return false;

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        ConfigureTargetEntity();

        //if (this.theEntity is EntityAliveSDX)
        //{
        //    EntityAliveSDX aliveTarget = theEntity as EntityAliveSDX;
        //    if (aliveTarget.CurrentOrder != EntityAliveSDX.Orders.Follow)
        //        return false;
        //}

 
        return (this.entityTarget != null);
    }

    //public override void Start()
    //{
    //    this.entityTargetPos = this.entityTarget.position;
    //    this.entityTargetVel = Vector3.zero;
    //    this.pathCounter = 0;
    //}

    public override bool Continue()
    {
        bool result = true;

        // If the entity has a cvar for a control mechanism, and its 0, then just return. 
        if (this.theEntity.Buffs.HasCustomVar(this.strControlMechanism) && this.theEntity.Buffs.GetCustomVar(this.strControlMechanism) == 0)
            return false;

        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
        {
            result = false;
        }
        else
        {
            EntityAlive Target = this.theEntity.otherEntitySDX;
            if (!Target)
                result = false;
        
        }

        return result;
    }

    public override void Update()
    {

        // No entity, so no need to do anything.
        if (this.entityTarget == null)
            return;

        // Find the location of the entity, and figure out where it's at.
        Vector3 position = this.entityTarget.position;
        Vector3 a = position - this.entityTargetPos;
        if (a.sqrMagnitude < 1f)
            this.entityTargetVel = this.entityTargetVel * 0.7f + a * 0.3f;

        this.entityTargetPos = position;

        this.theEntity.moveHelper.CalcIfUnreachablePos(position);

        float num2 = distanceToEntity * distanceToEntity;
        float targetXZDistanceSq = base.GetTargetXZDistanceSq(6);
        float num3 = position.y - this.theEntity.position.y;
        float num4 = Utils.FastAbs(num3);
        bool flag = targetXZDistanceSq <= num2 && num4 < 1f;

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());

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
