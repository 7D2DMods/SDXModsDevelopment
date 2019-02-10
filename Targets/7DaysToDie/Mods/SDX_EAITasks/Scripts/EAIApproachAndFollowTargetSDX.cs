using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;

public class EAIApproachAndFollowTargetSDX : EAIApproachAndAttackTarget
{

    private List<Entity> NearbyEntities = new List<Entity>();

    private float maxChaseTime;

    private bool hasHome;

    private bool isGoingHome;

    private Vector3 entityTargetPos;

    private Vector3 entityTargetVel;
    private int pathCounter;

    private float homeTimeout;
    private String strControlMechanism = "";
    private EAIBlockingTargetTask blockTargetTask;

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        this.MutexBits = 3;
        this.executeDelay = 0.1f;
    }

    public void DisplayLog(string strMessage)
    {
        Debug.Log(this.GetType().Name + ": " + strMessage);
    }

    public override void SetParams1(string _par1)
    {
        this.strControlMechanism = _par1;
    }

    public void ConfigureTargetEntity()
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

        // If there is an entity in bounds, then let this AI Task roceed. Otherwise, don't do anything with it.
        ConfigureTargetEntity();

        return (this.entityTarget != null);
    }

    public override void Start()
    {
        this.entityTargetPos = this.entityTarget.position;
        this.entityTargetVel = Vector3.zero;

        this.homeTimeout = ((!this.theEntity.IsSleeper) ? this.maxChaseTime : 90f);
        this.hasHome = (this.homeTimeout > 0f);
        this.isGoingHome = false;

        // If it doesn't have a chase position, put the entity to sleep where it's standing, if it's not a sleeper.
        if (this.theEntity.ChaseReturnLocation == Vector3.zero)
            this.theEntity.ChaseReturnLocation = ((!this.theEntity.IsSleeper) ? this.theEntity.position : this.theEntity.SleeperSpawnPosition);
        this.pathCounter = 0;
    }

    public override bool Continue()
    {
        bool result = false;
        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
        {
            result = false;
        }
        else
        {

            EntityAlive Target = this.theEntity.otherEntitySDX;
            if (this.isGoingHome)
            {
                result = (!Target && this.theEntity.ChaseReturnLocation != Vector3.zero);
            }
            else if (!Target)
            {
                result = false;
            }
            else if (Target != this.entityTarget)
            {
                result = false;
            }
            // If our master has an attack target, don't follow him anymore.
            else if (Target.GetAttackTarget() != null)
            {
                return false;
            }
            else if (this.entityTarget == null)
            {
                return false;
            }
            else
            {
                result = true;
            }
        }

        return result;
    }

    public override void Reset()
    {
        this.theEntity.navigator.clearPath();
        if (this.blockTargetTask != null)
            this.blockTargetTask.canExecute = false;

    }

    public override void Update()
    {
        if (this.hasHome)
        {
            if (this.isGoingHome)
            {
                Vector3 vector = this.theEntity.ChaseReturnLocation - this.theEntity.position;
                float y = vector.y;
                vector.y = 0f;
                float sqrMagnitude = vector.sqrMagnitude;
                if (sqrMagnitude <= 0.09f && Utils.FastAbs(y) < 1.5f)
                {
                    Vector3 chaseReturnLocation = this.theEntity.ChaseReturnLocation;
                    chaseReturnLocation.y = this.theEntity.position.y;
                    this.theEntity.SetPosition(chaseReturnLocation, true);
                    this.theEntity.ChaseReturnLocation = Vector3.zero;
                    if (this.theEntity.IsSleeper)
                    {
                        this.theEntity.ResumeSleeperPose();
                    }
                    return;
                }
                if (--this.pathCounter <= 0 && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
                {
                    this.pathCounter = 60;
                    PathFinderThread.Instance.FindPath(this.theEntity, this.theEntity.ChaseReturnLocation, this.theEntity.GetMoveSpeed(), false, this);
                }
                return;
            }
            else
            {
                this.homeTimeout -= 0.05f;
                if (this.homeTimeout <= 0f)
                {
                    if (this.blockTargetTask == null)
                    {
                        List<EAIBlockingTargetTask> targetTasks = this.manager.GetTargetTasks<EAIBlockingTargetTask>();
                        if (targetTasks != null)
                        {
                            this.blockTargetTask = targetTasks[0];
                        }
                    }
                    if (this.blockTargetTask != null)
                    {
                        this.blockTargetTask.canExecute = true;
                    }
                    this.theEntity.otherEntitySDX = null;
                    this.theEntity.SetLookPosition(Vector3.zero);
                    this.theEntity.PlayGiveUpSound();
                    this.pathCounter = 0;
                    this.isGoingHome = true;
                    return;
                }
            }
        }

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

        float distanceToEntity = 2f;
        float num2 = distanceToEntity * distanceToEntity;
        float targetXZDistanceSq = this.GetTargetXZDistanceSq(6);
        float num3 = position.y - this.theEntity.position.y;
        float num4 = Utils.FastAbs(num3);
        bool flag = targetXZDistanceSq <= num2 && num4 < 1f;

        // Let the entity keep looking at you, otherwise it may just sping around.
        this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());

        // num is used to determine how close and comfortable the entity approaches you, so let's make sure they respect some personal space
        if (distanceToEntity < 1)
            distanceToEntity = 4;

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

    private float GetTargetXZDistanceSq(int estimatedTicks)
    {
        Vector3 vector = this.entityTarget.position;
        vector += this.entityTargetVel * (float)estimatedTicks;
        Vector3 vector2 = this.theEntity.position + this.theEntity.motion * (float)estimatedTicks - vector;
        vector2.y = 0f;
        return vector2.sqrMagnitude;
    }

    private Vector3 GetMoveToLocation(float maxDist)
    {
        Vector3 vector = this.entityTarget.position;
        vector += this.entityTargetVel * 6f;
        vector = this.entityTarget.world.FindSupportingBlockPos(vector);
        if (maxDist > 0f)
        {
            Vector3 vector2 = new Vector3(this.theEntity.position.x, vector.y, this.theEntity.position.z);
            Vector3 vector3 = vector - vector2;
            float magnitude = vector3.magnitude;
            if (magnitude < 3f)
            {
                if (magnitude <= maxDist)
                {
                    float num = vector.y - this.theEntity.position.y;
                    if (num > 1.5f)
                    {
                        return vector;
                    }
                    return vector2;
                }
                else
                {
                    vector3 *= maxDist / magnitude;
                    Vector3 vector4 = vector - vector3;
                    vector4.y += 0.51f;
                    Vector3i pos = World.worldToBlockPos(vector4);
                    int type = this.entityTarget.world.GetBlock(pos).type;
                    Block block = Block.list[type];
                    if (!block.IsPathSolid && Physics.Raycast(vector4, Vector3.down, 1.02f, 1082195968))
                    {
                        return vector4;
                    }
                }
            }
        }
        return vector;
    }
}
