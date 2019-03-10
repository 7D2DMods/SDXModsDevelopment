using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
class EAILootLocation: EAIBase
{
    PrefabInstance prefab;
    List<TileEntityLootContainer> lstTileContainers = new List<TileEntityLootContainer>();
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        EntityAliveSDX entity = this.theEntity as EntityAliveSDX;
        if (entity == null)
            return false;

        if (entity.Buffs.HasCustomVar("CurrentOrder"))
        {
            if (entity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Loot)
            {
                // Check if we have a prefab and isn't owned by a player.
                if (FindBoundsOfPrefab())
                {
                    this.lstTileContainers = ScanForTileEntityInList();
                    if ( this.lstTileContainers.Count > 0 )
                        return true;
                }
            }
        }
        return false;
    }

    public override bool Continue()
    {
        if (theEntity.Buffs.HasCustomVar("CurrentOrder") && (theEntity.Buffs.GetCustomVar("CurrentOrder") == (float)EntityAliveSDX.Orders.Loot))
        {
            if (this.theEntity.HasInvestigatePosition)
                return true;
            
            
            return true;
        }

        return false;
    }
    public bool FindBoundsOfPrefab()
    {
       var pos = this.theEntity.position;
       prefab = GameManager.Instance.prefabLODManager.prefabsAroundNear.Values.FirstOrDefault(d => pos.x >= d.boundingBoxPosition.x && pos.x < d.boundingBoxPosition.x + d.boundingBoxSize.x && pos.z >= d.boundingBoxPosition.z && pos.z < d.boundingBoxPosition.z + d.boundingBoxSize.z);
        if (prefab == null)
            return false;

        if (prefab.CheckForAnyPlayerHome(this.theEntity.world) != GameUtils.EPlayerHomeType.None)
            return false;

        var prefabBounds = prefab.boundingBoxSize;

        return true;
    }

    public virtual List<TileEntityLootContainer> ScanForTileEntityInList()
    {
        // Otherwise, search for your new home.
        Vector3i blockPosition = this.theEntity.GetBlockPosition();
        List<TileEntityLootContainer> LocalContainers = new List<TileEntityLootContainer>();
        var minX = prefab.boundingBoxPosition.x;
        var maxX = prefab.boundingBoxPosition.x + prefab.boundingBoxSize.x - 1;

        var minZ = prefab.boundingBoxPosition.z;
        var maxZ = prefab.boundingBoxPosition.z + prefab.boundingBoxSize.z - 1;

        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
                var chunkPos = chunk.GetWorldPos();
                var worldX = chunkPos.x + i;
                var worldZ = chunkPos.z + j;

                // Out of bounds
                if (worldX < minX || worldX > maxX || worldZ < minZ || worldZ > maxZ)
                    continue;

                if (chunk != null)
                {
                    // Grab all the Tile Entities in the chunk
                    DictionaryList<Vector3i, TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntityLootContainer tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            BlockValue block = theEntity.world.GetBlock(tileEntity.ToWorldPos());
                            if (tileEntity.bTouched)
                                continue;

                            LocalContainers.Add(tileEntity);
                        }
                    }
                }
            }
        }

        return LocalContainers;
    }

    public void FindNearestContainer(List<TileEntityLootContainer> localContainers)
    {
    
        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (TileEntityLootContainer block in localContainers)
        {
            float dist = Vector3.Distance(block.ToWorldPos().ToVector3(), currentPos);
            if (dist < minDist)
            {
                tMin = block.ToWorldPos().ToVector3();
                minDist = dist;
            }
        }
        this.theEntity.SetInvestigatePosition(tMin, 1200);
        
    }

}

