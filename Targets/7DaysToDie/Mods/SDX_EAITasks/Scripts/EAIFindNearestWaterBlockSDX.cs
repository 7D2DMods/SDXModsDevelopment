using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIFindNearestWaterBlockSDX : EAIApproachSpot
{
    List<String> Incentives = new List<String>();
    int MaxDistance = 20;
    public int investigateTicks;

    Vector3i LastBlockPosition = new Vector3i(0, 0, 0);
    public  bool hadPath;
    private bool blDisplayLog = true;


    public void DisplayLog(String strMessage)
    {     
        EntityAliveSDX myEntity = this.theEntity as EntityAliveSDX;
        if ( myEntity ) 
            blDisplayLog = myEntity.IsEntityDebug();
        
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
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

    // second parameter is the maximum distance to search. Default is 10 blocks.
    public override void SetParams2(string _par2)
    {
        Int32.TryParse(_par2, out this.MaxDistance);
        if (this.MaxDistance == 0)
            this.MaxDistance = 10;
    }
    public override bool CanExecute()
    {
        if (! CheckIncentive())
            return false;

      
        if (CheckForBlock() == false)
            return false;

        return base.CanExecute();
        //DisplayLog(" Setting Investigation Position to: " + this.LastBlockPosition);
        //this.theEntity.SetInvestigatePosition(this.LastBlockPosition.ToVector3(), 60);
            
        //return false;
    }

    public virtual  bool CheckIncentive()
    {
        foreach( String strIncentive in this.Incentives)
        {
            if (this.theEntity.Buffs.HasBuff(strIncentive))
                return true;
        }

        // Doesn't match any incentive, so reset its invesgitate position.
        this.theEntity.SetInvestigatePosition(Vector3.zero, 0);
        return false;

    }

    // Don't continue to search if it no longer matches the incentive.
    public override bool Continue()
    {
        if ( CheckIncentive() == false)
            return false;
   
        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            DisplayLog("Continue(): No AI Task Found");
            return false;
        }

        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
            {
                DisplayLog("No Investigation Position. Not Searching anymore.");
                return false;
            }
            float sqrMagnitude = (this.LastBlockPosition.ToVector3() - this.theEntity.InvestigatePosition).sqrMagnitude;
            if (sqrMagnitude >= 4f)
            {
                DisplayLog(" Too Far away. Not searching anymore.");
                return false;
            }
        }
        float sqrMagnitude2 = (this.LastBlockPosition.ToVector3() - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 1f || (path != null && path.isFinished()))
        {
            DisplayLog("Continue(): I am at the block, or I have given up.");
            PerformAction();
            return false;
        }
        return true;
    }

    // Virtual methods to overload, so we can choose what kind of action to take.
    public virtual void PerformAction()
    {
        if (this.theEntity.inventory.holdingItem.Actions[0] != null)
        {
            DisplayLog(" Before: ");
            DisplayLog( this.theEntity.ToString());
            DisplayLog(" Executing: " + this.theEntity.inventory.holdingItem.GetItemName());

  
            // Look at the water, then execute the action on the empty jar.
            this.theEntity.SetLookPosition(this.LastBlockPosition.ToVector3());
            if (this.theEntity.inventory.holdingItem.Actions[1] != null)
            {
                DisplayLog(" Executing drinkAnimalWater ");
                this.theEntity.inventory.holdingItem.Actions[1].ExecuteAction(this.theEntity.inventory.holdingItemData.actionData[1], true);
            }

          
            DisplayLog(" After: ");
            DisplayLog(this.theEntity.ToString());
        }
    }


    // Virtual method to find the target for what we are looking for. This one is for liquid.
    public virtual bool CheckForBlock()
    {
        // if the last source of water is still available, then re-use that, rather than scan.
        if (Block.list[theEntity.world.GetBlock(this.LastBlockPosition).type].blockMaterial.IsLiquid  )
        {
            return true;
        }
        Vector3i blockPosition = theEntity.GetBlockPosition();
  


        for (var x = (int)blockPosition.x - this.MaxDistance; x <= blockPosition.x + this.MaxDistance; x++)
        {
            for (var z = (int)blockPosition.z - this.MaxDistance; z <= blockPosition.z + this.MaxDistance; z++)
            {
                for (var y = (int)blockPosition.y - this.MaxDistance; y <= blockPosition.y + this.MaxDistance; y++)
                {
                    BlockValue checkBlock = theEntity.world.GetBlock(x, y, z);
                    if (Block.list[checkBlock.type].blockMaterial.IsLiquid)
                    {
                        DisplayLog(" CheckForBlock(): Found Water Block: " + checkBlock.ToString());
                        this.LastBlockPosition.x = x;
                        this.LastBlockPosition.y = y;
                        this.LastBlockPosition.z = z;
                        this.theEntity.SetInvestigatePosition(this.LastBlockPosition.ToVector3(), 400);
                        return true;
                    }
                }
            }
        }       

        DisplayLog("CheckForBlock(): No Water Found");
        return false;
    }
}

