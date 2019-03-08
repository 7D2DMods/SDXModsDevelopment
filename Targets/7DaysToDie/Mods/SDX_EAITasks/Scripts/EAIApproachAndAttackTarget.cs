using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using GamePath;

class EAIApproachAndAttackTargetSDX : EAIApproachAndFollowTargetSDX
{

    List<String> lstIncentives = new List<String>();
    private List<Entity> NearbyEntities = new List<Entity>();
    private float maxChaseTime;

    float distanceToEntity = UnityEngine.Random.Range(2f, 5.0f);
    private int relocateTicks;
    private Vector3 entityTargetPos;
    private Vector3 entityTargetVel;
    private int pathCounter;

    public int attackTimeout= 5;

     public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || this.theEntity.Jumping)
            return false;

        this.entityTarget = this.theEntity.GetRevengeTarget();
        if (this.entityTarget)
            return true;

        this.entityTarget = this.theEntity.GetAttackTarget();   
        if (this.entityTarget == null)
            return false;

        return true;
    }

    public override bool Continue()
    {
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
            return false;

        this.entityTarget = this.theEntity.GetRevengeTarget();
        if (this.entityTarget && this.entityTarget.IsAlive() )
            return true;

        this.entityTarget = this.theEntity.GetAttackTarget();
        if (this.entityTarget == null)
            return false;

        if (!this.entityTarget.IsAlive())
            return false;

        return true;
    }

    public override void Update()
    {
     
        if (this.entityTarget == null)
            return;

        if (this.relocateTicks > 0)
        {
            if (!this.theEntity.navigator.noPathAndNotPlanningOne())
            {
                this.relocateTicks--;
                this.theEntity.moveHelper.SetFocusPos(this.entityTarget.position);
                return;
            }
            this.relocateTicks = 0;
        }
        Vector3 position = this.entityTarget.position;
        Vector3 a = position - this.entityTargetPos;
        if (a.sqrMagnitude < 1f)
        {
            this.entityTargetVel = this.entityTargetVel * 0.7f + a * 0.3f;
        }
        this.entityTargetPos = position;
        this.attackTimeout--;
     
        this.theEntity.moveHelper.CalcIfUnreachablePos(position);
        ItemAction itemAction = this.theEntity.inventory.holdingItem.Actions[0];
        float num = 1.095f; 
        float num2 = num * num;
        float targetXZDistanceSq = this.GetTargetXZDistanceSq(6);
        float num3 = position.y - this.theEntity.position.y;
        float num4 = Utils.FastAbs(num3);
        bool flag = targetXZDistanceSq <= num2 && num4 < 1f;
        if (!flag )
        {
            if (!PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
            {
                PathEntity path = this.theEntity.navigator.getPath();
                if (path != null && path.NodeCountRemaining() <= 2)
                {
                    this.pathCounter = 0;
                }
            }
            if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
            {
                this.pathCounter = 6 + this.theEntity.GetRandom().Next(10);
                Vector3 moveToLocation = this.GetMoveToLocation(num);
                PathFinderThread.Instance.FindPath(this.theEntity, moveToLocation, this.theEntity.GetMoveSpeedAggro(), true, this);
            }
        }
        if (this.theEntity.Climbing)
        {
            return;
        }
        bool flag2 = this.theEntity.CanSee(this.entityTarget);
        this.theEntity.SetLookPosition((!flag2 || this.theEntity.IsBreakingBlocks) ? Vector3.zero : this.entityTarget.getHeadPosition());
        if (!flag)
        {
            if (this.theEntity.navigator.noPathAndNotPlanningOne() && num3 < 2.1f)
            {
                Vector3 moveToLocation2 = this.GetMoveToLocation(num);
                this.theEntity.moveHelper.SetMoveTo(moveToLocation2, true);
            }
        }
        else
        {
            this.theEntity.navigator.clearPath();
            this.theEntity.moveHelper.Stop();
            this.pathCounter = 0;
        }
        float num5 =  1.095f;
        float num6 = num5 * num5;
        if (targetXZDistanceSq > num6 || num4 >= 1.25f)
        {
            DisplayLog(" TargetXZDistanceSq: " + targetXZDistanceSq + " > num6 " + num6 + " or num4 > " + num4 + " 1.25f");
            return;
        }
        this.theEntity.IsBreakingBlocks = false;
        this.theEntity.IsBreakingDoors = false;
        if (!this.theEntity.bodyDamage.HasNoArmsAndLegs)
        {
            this.theEntity.RotateTo(position.x, position.y, position.z, 30f, 30f);
        }
      
        bool flag3 = this.theEntity.GetDamagedTarget() == this.entityTarget || (this.entityTarget != null && this.entityTarget.GetDamagedTarget() == this.theEntity);
        if (flag3)
        {
     
            this.theEntity.ClearDamagedTarget();
            if (this.entityTarget)
                this.entityTarget.ClearDamagedTarget();
        }
        if (this.attackTimeout > 0)
            return;

        if (this.manager.groupCircle > 0f)
        {
            Entity targetIfAttackedNow = this.theEntity.GetTargetIfAttackedNow();
            if (targetIfAttackedNow != this.entityTarget && (!this.entityTarget.AttachedToEntity || this.entityTarget.AttachedToEntity != targetIfAttackedNow))
            {
                if (targetIfAttackedNow != null)
                {
                    this.relocateTicks = 45;
                    Vector3 vector2 = (this.theEntity.position - position).normalized * (num5 + 1.1f);
                    float num7 = base.RandomFloat * 28f + 18f;
                    if (base.RandomFloat < 0.5f)
                    {
                        num7 = -num7;
                    }
                    vector2 = Quaternion.Euler(0f, num7, 0f) * vector2;
                    Vector3 target = position + vector2;
                    PathFinderThread.Instance.FindPath(this.theEntity, target, this.theEntity.GetMoveSpeedAggro(), true, this);
                }

                return;
            }
        }
        this.theEntity.SleeperSupressLivingSounds = false;
        if (this.theEntity.Attack(false))
        {
            this.attackTimeout = this.theEntity.GetAttackTimeoutTicks();
            this.theEntity.Attack(true);
        }
    }
    public bool CheckSurroundingEntities(EntityAlive leader)
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
                if (x.GetAttackTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by " + x.ToString());
                    this.theEntity.SetAttackTarget(x, 1200);
                    return true;
                }

                if (x.GetRevengeTarget() == leader)
                {
                    DisplayLog(" My leader is being avenged by " + x.ToString());
                    this.theEntity.SetAttackTarget(x, 1200);
                    return true;
                }

                if (x.GetDamagedTarget() == leader)
                {
                    DisplayLog(" My leader is being attacked by something that damaged it " + x.ToString());
                    this.theEntity.SetAttackTarget(x, 1200);
                    return true;
                }
            }
        }

        return false;
    }

    public override void SetParams1(string _par1)
    {

    }
    // Allow params to be a comma-delimited list of various incentives, such as item name, buff, or cvar.
    public override void SetParams2(string _par1)
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

        return result;
    }

}


