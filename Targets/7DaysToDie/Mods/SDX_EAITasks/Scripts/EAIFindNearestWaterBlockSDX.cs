using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIFindNearestWaterBlockSDX : EAIApproachSpot
{

    List<String> Incentives = new List<String>();
    int MaxDistance = 20;
    public int investigateTicks;

   // Vector3i LastBlockPosition = new Vector3i(0, 0, 0);
    public  bool hadPath;
    private bool blDisplayLog = true;
    private Vector3 investigatePos;
    private Vector3 seekPos;

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
        if (!CheckIncentive() || !CheckForWaterBlock())
            return false;
        
        return base.CanExecute();
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

    // Don't continue to search if it no longer matches the incentive.
    public override bool Continue()
    {
        if ( CheckIncentive() == false)
            return false;

        //PathNavigate navigator = this.theEntity.navigator;
        //PathEntity path = navigator.getPath();
        
        //if ( (path != null && path.isFinished()) || (this.seekPos - this.theEntity.position).sqrMagnitude < 2f)
        //{
        //        PerformAction();
        //        return false;
        //}
        bool result = base.Continue();
        PerformAction();
        return result;  
    }

    // Virtual methods to overload, so we can choose what kind of action to take.
    public virtual void PerformAction()
    {
        if (this.theEntity.inventory.holdingItem.Actions[1] != null)
        {
            // Look at the water, then execute the action on the empty jar.
            this.theEntity.SetLookPosition(seekPos);
            if (this.theEntity.inventory.holdingItem.Actions[1] != null)
            {
                this.theEntity.inventory.holdingItem.Actions[1].ExecuteAction(this.theEntity.inventory.holdingItemData.actionData[1], true);
                this.theEntity.SetInvestigatePosition(Vector3.zero, 0);

            }
        }
    }


    // Virtual method to find the target for what we are looking for. This one is for liquid.
    public virtual bool CheckForWaterBlock()
    {
        if (this.theEntity.InvestigatePosition == this.seekPos)
            return true;

        // if the last source of water is still available, then re-use that, rather than scan.
        //if (Block.list[theEntity.world.GetBlock(seekPos).type].blockMaterial.IsLiquid)
        //{
        //    this.theEntity.SetInvestigatePosition(seekPos, 40);
        //    return true;
        //}
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
                        seekPos.x = x;
                        seekPos.y = y;
                        seekPos.z = z;

                        //this.LastBlockPosition.x = x;
                        //this.LastBlockPosition.y = y;
                        //this.LastBlockPosition.z = z;
                        this.theEntity.SetInvestigatePosition(seekPos, 1200);
                        return true;
                    }
                }
            }
        }       

        DisplayLog("CheckForBlock(): No Water Found");
        return false;
    }
}

