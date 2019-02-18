using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIFindNearestFoodBlockSDX : EAIFindNearestWaterBlockSDX
{
    List<String> Incentives = new List<String>();
    List<String> FoodBlocks = new List<String>();
    int MaxDistance = 20;

    Vector3i LastBlockPosition = new Vector3i(0, 0, 0);
    private bool blDisplayLog = false;

    public bool CheckIncentive(String strBlock)
    {
        foreach (String strIncentive in this.Incentives)
        {
            if (strIncentive == strBlock)
                return true;
        }
        return false;

    }

    public override void SetParams2(string _par2)
    {
        string[] array = _par2.Split(new char[]
    {
            ','
    });
        for (int i = 0; i < array.Length; i++)
        {
            if (this.FoodBlocks.Contains(array[i].ToString()))
                continue;
            this.FoodBlocks.Add(array[i].ToString());
        }
    }

    // Virtual method to find the target for what we are looking for. This one is for liquid.
    public override bool CheckForWaterBlock()
    {
        
        Vector3i blockPosition = theEntity.GetBlockPosition();
        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntity tileEntity = tileEntities.list[k] as TileEntity;
                        if (tileEntity != null)
                        {
                            DisplayLog("Checking Tile Entity: " + tileEntity.ToString());
                            // Check the block to see if it's the one we want
                            BlockValue block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            if (CheckIncentive(block.Block.GetBlockName()) == true)
                            {
                                this.theEntity.SetInvestigatePosition(this.LastBlockPosition.ToVector3(), 40);

                                return true;
                            }
                        }
                    }
                }
            }
        }


        return false;
    }
}

