using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIFindNearestWaterBlockSDX : EAIApproachSpot
{

    List<String> Incentives = new List<String>();
    int MaxDistance = 20;
    public int investigateTicks;
    List<Vector3> lstWaterBlocks = new List<Vector3>();

   // Vector3i LastBlockPosition = new Vector3i(0, 0, 0);
    public  bool hadPath;
    private bool blDisplayLog = true;
    private Vector3 investigatePos;
    private Vector3 seekPos;
    private int pathRecalculateTicks;

    public void DisplayLog(String strMessage)
    {     
        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if ( myEntity ) 
            blDisplayLog = myEntity.IsEntityDebug();
        
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        this.MutexBits = 3;
        this.executeDelay = 0.5f;
    }
    // Accept multiple parameters, as a buff, to make it look for water.
    public override void SetParams1(string _par1)
    {
        string[] array = _par1.Split(new char[]
        {
            ','
        });
        for (int i = 0; i < array.Length; i++)
        {
            if (this.Incentives.Contains(array[i].ToString()))
                continue;
            this.Incentives.Add(array[i].ToString());
        }
    }

    public override bool CanExecute()
    {
        // If there's no buff incentive, or no nearby water block, don't bother looking for water.
        if (!CheckIncentive())
            return false;

        if (!this.theEntity.HasInvestigatePosition)
        {
            DisplayLog("I do not have an investigate position, but I have an incentive to look. Investigative Ticks: " + this.theEntity.GetInvestigatePositionTicks() );
            CheckForWaterBlock();
            if (!this.theEntity.HasInvestigatePosition)
            {
                DisplayLog("\t After rechecking, I could not find a water block");
                return false;
            }
        }
        if (this.theEntity.IsSleeping)
        {
            DisplayLog(" I am sleeping");
            return false;
        }

        
        this.investigatePos = this.theEntity.InvestigatePosition;
        this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
        DisplayLog(" Investigate Pos: " + this.investigatePos + " Seek Position: " + this.seekPos);
        DisplayLog(" Found Water Block and have Incentive to Eat: " + this.ToString());
        return true;

        // Otherwise, follow the conditions of the base class and check if there's an investigative position on the entity
        // This is set in the CheckForWaterBlock call.
        bool result = base.CanExecute();
        if (result)
            DisplayLog(" Found Water Block and have Incentive to Eat: " + this.ToString());
        else
            DisplayLog(" I cannot continue to search. base Class says No");
    }
 
    public override void Start()
    {
        this.hadPath = false;
        this.updatePath();
    }
    public override void Reset()
    {
        this.theEntity.navigator.clearPath();
        this.theEntity.SetLookPosition(Vector3.zero);
        this.manager.lookTime = 5f + UnityEngine.Random.value * 3f;
        this.manager.interestDistance = 2f;
    }
    public override void Update()
    {
        PathEntity path = this.theEntity.navigator.getPath();
        if (path != null)
        {
            this.hadPath = true;
            this.theEntity.moveHelper.CalcIfUnreachablePos(this.seekPos);
        }
        Vector3 lookPosition = this.investigatePos;
       // lookPosition.y += 0.8f;
        this.theEntity.SetLookPosition(lookPosition);
        if (--this.pathRecalculateTicks <= 0)
        {
            this.updatePath();
        }
    }
    private void updatePath()
    {
        if (PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
            return;

        this.pathRecalculateTicks = 40 + this.theEntity.GetRandom().Next(20);
        PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeedAggro(), false, this);
    }

    public virtual  bool CheckIncentive()
    {
        foreach( String strIncentive in this.Incentives)
        {
            if (this.theEntity.Buffs.HasBuff(strIncentive))
                return true;
        }

        return false;

    }

    public override bool Continue()
    {
        if (CheckIncentive() == false)
            return false;

        if (this.theEntity.IsInWater(this.theEntity.position.x, this.theEntity.position.y, this.theEntity.position.z)) 
        {
            DisplayLog(" I am standing in water. Attempting to drink.");

            return PerformAction();
        }
        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            DisplayLog(" No Path");
            return false;
        }
        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
            {
                DisplayLog(" Entity has no Investigate Position");
                return false;
            }
            float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            if (sqrMagnitude >= 4f)
            {
                DisplayLog("magnitude is too high.");
                return false;
            }
        }
   
        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 4f || (path != null && path.isFinished()))
            return PerformAction();
        return true;
    }

    Vector3 GetClosesWater()
    {
        Vector3 tMin = new Vector3();
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (Vector3 water in this.lstWaterBlocks)
        {
            float dist = Vector3.Distance( water, currentPos);
            if (dist < minDist)
            {
                tMin = water;
                minDist = dist;
            }
        }
        return tMin;
    }
    // Virtual methods to overload, so we can choose what kind of action to take.
    public virtual bool PerformAction()
    {
        DisplayLog(" Trying to PerformAction() ");
        if (this.theEntity.inventory.holdingItem.Actions[1] != null)
        {
            BlockValue checkBlock = theEntity.world.GetBlock( new Vector3i( seekPos.x, seekPos.y, seekPos.z));
            if (checkBlock.Block.blockMaterial.IsLiquid)
            {
                // Look at the water, then execute the action on the empty jar.
                this.theEntity.SetLookPosition(seekPos);
                if (this.theEntity.inventory.holdingItem.Actions[1] != null)
                    this.theEntity.inventory.holdingItem.Actions[1].ExecuteAction(this.theEntity.inventory.holdingItemData.actionData[1], true);

                return false;
            }
            else
            {
                DisplayLog("Entity is not looking at water.");
            }
        }

        return true;
    }



    // Virtual method to find the target for what we are looking for. This one is for liquid.
    public virtual bool CheckForWaterBlock()
    {
        Vector3i blockPosition = theEntity.GetBlockPosition();
        Vector3i WaterPosition = new Vector3i();
        
        for (var x = (int)blockPosition.x - this.MaxDistance; x <= blockPosition.x + this.MaxDistance; x++)
        {
            for (var z = (int)blockPosition.z - this.MaxDistance; z <= blockPosition.z + this.MaxDistance; z++)
            {
                for (var y = (int)blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    WaterPosition.x = x;
                    WaterPosition.y = y;
                    WaterPosition.z = z;

                    BlockValue checkBlock = theEntity.world.GetBlock( WaterPosition);
                    if (Block.list[checkBlock.type].blockMaterial.IsLiquid)
                        this.lstWaterBlocks.Add(WaterPosition.ToVector3());
                }
            }
        }

        Vector3 WaterBlock = GetClosesWater();
        DisplayLog("Closes Water Block: " + WaterBlock);
        if (WaterBlock != Vector3.zero)
        {
            this.theEntity.SetInvestigatePosition(WaterBlock, 1200);
            DisplayLog("Water Block: " + WaterBlock);
            DisplayLog("Has Investigative Spot been set? : " + this.theEntity.HasInvestigatePosition);
            DisplayLog(" Investigative Spot: " + this.theEntity.InvestigatePosition);
            return true;
        }
        DisplayLog("CheckForBlock(): No Water Found");
        return false;
    }

   
}

