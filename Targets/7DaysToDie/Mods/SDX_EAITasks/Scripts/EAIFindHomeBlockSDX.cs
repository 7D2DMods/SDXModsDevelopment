using System;
using System.Collections.Generic;
using UnityEngine;

class EAIFindHomeBlockSDX : EAIBase
{
    String strHomeBlock;
    String strHomeBuff;
    private String strControlMechanism = "";

    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + strMessage);
    }

    public override void SetParams1(string _par1)
    {
        this.strHomeBlock = _par1;
    }

    public override void SetParams2(string _par2)
    {
        this.strHomeBuff = _par2;
    }
    public override bool CanExecute()
    {
        if (String.IsNullOrEmpty(this.strHomeBlock))
            return false;

        if (strHomeBlock == theEntity.world.GetBlock(theEntity.getHomePosition().position).Block.GetBlockName())
            return false;

        return true;
    }

    public override bool Continue()
    {
        if (String.IsNullOrEmpty(this.strHomeBlock))
            return false;

        if (strHomeBlock == theEntity.world.GetBlock(theEntity.getHomePosition().position).Block.GetBlockName())
            return false;

        return true;
    }
    public override void Update()
    {
        // Otherwise, search for your new home.
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
                        TileEntityLootContainer tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            BlockValue block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            DisplayLog(" Found TileEntity: " + block.Block.GetBlockName());

                            // If it's not the entities home block, move to the next one.
                            if (block.Block.GetBlockName() != this.strHomeBlock)
                                continue;

                            DisplayLog("Found a home Block: " + block.Block.GetBlockName());
                            Block block2 = Block.list[block.type];
                            if (block2.RadiusEffects != null)
                            {
                                float distanceSq = theEntity.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                {
                                    BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                    DisplayLog(" RadiusEffect: " + blockRadiusEffect.variable + " The Buff: " + this.strHomeBuff);
                                    if (blockRadiusEffect.variable == strHomeBuff)
                                    {
                                        if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius)
                                        {
                                            
                                            theEntity.setHomeArea(tileEntity.ToWorldPos(), 15);
                                            DisplayLog("Setting Home Position: " + theEntity.getHomePosition().ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


    }

  
}

