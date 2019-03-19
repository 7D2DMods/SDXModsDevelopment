using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;
using GamePath;

class EAILootLocationSDX : EAIApproachSpot
{
    private Vector3 investigatePos;

    private Vector3 seekPos;
    private bool hadPath;
    private int investigateTicks;

    PrefabInstance prefab;
    List<TileEntityLootContainer> lstTileContainers = new List<TileEntityLootContainer>();
    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }
    public override bool CanExecute()
    {
        DisplayLog("CanExecute()");
        EntityAliveSDX entity = this.theEntity as EntityAliveSDX;
        if (entity == null)
        {
            DisplayLog(" I am not an EntityAliveSDX");
            return false;
        }
        if (this.theEntity.GetAttackTarget() != null || this.theEntity.GetRevengeTarget() != null)
        {
            DisplayLog(" I have an attack target or a revenger target. Not looting.");
            return false;
        }

        if (this.theEntity.HasInvestigatePosition)
            return true;

        if (FindBoundsOfPrefab())
        {
            ScanForTileEntityInList();
            return FindNearestContainer();
        }
        return false;
    }


    public override bool Continue()
    {
        if (this.theEntity.GetAttackTarget() != null || this.theEntity.GetRevengeTarget() != null)
        {
            DisplayLog(" I have an attack target or Revenge Target. Not Looting.");
            return false;
        }
        if (theEntity.Buffs.HasCustomVar("CurrentOrder") && (theEntity.Buffs.GetCustomVar("CurrentOrder") != (float)EntityAliveSDX.Orders.Loot))
            return false;

        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            return false;
        }
        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
                FindNearestContainer();

            float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            if (sqrMagnitude >= 4f)
            {
                return false;
            }
        }

        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 2f || (path != null && path.isFinished()))
        {
            DisplayLog("I'm at the loot container.");
            CheckContainer();
     
            this.theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
        return true;
    }

    public bool CheckContainer()
    {
        this.theEntity.SetLookPosition(seekPos);

        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return false; // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return false; // Missed the target. Overlooking?

        DisplayLog(" Looking at: " + this.seekPos + " My position is: " + this.theEntity.position);
        TileEntityLootContainer tileEntityLootContainer = this.theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
        if (tileEntityLootContainer == null)
        {
            DisplayLog("No Loot container here.");
            return false;

        }

        GetItemFromContainer(tileEntityLootContainer);
        if (tileEntityLootContainer.IsEmpty())
            DisplayLog(" Looted Container.");
        else
            DisplayLog("Did not loot the container.");

        return false;

    }
    public bool FindBoundsOfPrefab()
    {
        var pos = this.theEntity.position;
        prefab = GameManager.Instance.prefabLODManager.prefabsAroundNear.Values.FirstOrDefault(d => pos.x >= d.boundingBoxPosition.x && pos.x < d.boundingBoxPosition.x + d.boundingBoxSize.x && pos.z >= d.boundingBoxPosition.z && pos.z < d.boundingBoxPosition.z + d.boundingBoxSize.z);
        if (prefab == null)
        {
            DisplayLog(" I am not in a prefab. Returning false.");
            return false;
        }

        if (prefab.CheckForAnyPlayerHome(this.theEntity.world) != GameUtils.EPlayerHomeType.None)
        {
            DisplayLog(" This is a player's home. Not looting.");
            return false;
        }

        var prefabBounds = prefab.boundingBoxSize;

        return true;
    }

    public void ScanForTileEntityInList()
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

                            DisplayLog(" Loot Container: " + tileEntity.ToString());
                            this.lstTileContainers.Add(tileEntity);
                        }
                    }
                }
            }
        }
    }



    // Grab a single item from the storage box, and remmove it.
    public void GetItemFromContainer(TileEntityLootContainer tileLootContainer)
    {
        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return;  // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return; // Missed the target. Overlooking?

        Vector3i blockPos = tileLootContainer.ToWorldPos();
        this.lstTileContainers.Remove(tileLootContainer);

        DisplayLog("Checking Loot Container");
        if (tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.items;
            DisplayLog(" Loot Container has this many items: " + tileLootContainer.items.Length);
            tileLootContainer.bWasTouched = tileLootContainer.bTouched;
            this.theEntity.world.GetGameManager().TELockServer(Voxel.voxelRayHitInfo.hit.clrIdx, blockPos, tileLootContainer.entityId, this.theEntity.entityId);

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty()) // nothing in the slot
                    continue;

                DisplayLog(" Adding Item: " + array[i].itemValue.ToString());
                this.theEntity.inventory.AddItem(array[i]);
                DisplayLog(" Removing ITem");
                tileLootContainer.RemoveItem(array[i].itemValue);

            }
            this.theEntity.world.GetGameManager().TEUnlockServer(Voxel.voxelRayHitInfo.hit.clrIdx, blockPos, tileLootContainer.entityId);

        }

    }
    public bool FindNearestContainer()
    {

        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (TileEntityLootContainer block in this.lstTileContainers)
        {
            float dist = Vector3.Distance(block.ToWorldPos().ToVector3(), currentPos);
            if (dist < minDist)
            {
                tMin = block.ToWorldPos().ToVector3();
                minDist = dist;
            }


        }

        if (tMin == Vector3.zero)
            return false;

        this.theEntity.SetInvestigatePosition(tMin, 1200);
        DisplayLog(" Investigate Pos: " + this.investigatePos + " Current Seek Time: " + investigateTicks + " Max Seek Time: " + this.theEntity.GetInvestigatePositionTicks() + " Seek Position: " + this.seekPos + " Target Block: " + this.theEntity.world.GetBlock(new Vector3i(this.investigatePos)).Block.GetBlockName());
        this.investigatePos = this.theEntity.InvestigatePosition;
        this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
        return true;
    }

}

