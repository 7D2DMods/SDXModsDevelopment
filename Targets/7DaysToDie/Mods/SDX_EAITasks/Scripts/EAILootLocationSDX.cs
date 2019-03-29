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
    private EntityAliveSDX entityAliveSDX;

    PrefabInstance prefab;
    List<TileEntityLootContainer> lstTileContainers = new List<TileEntityLootContainer>();
    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " :" + this.theEntity.EntityName + ": " + strMessage);
    }


    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        entityAliveSDX = (_theEntity as EntityAliveSDX);
    }
    public override bool CanExecute()
    {
        DisplayLog("CanExecute()");
        bool result = false;
        if (entityAliveSDX)
        {
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Loot);
            DisplayLog("CanExecute() Loot Task? " + result);
            if (result == false)
                return false;
        }

        if (FindBoundsOfPrefab())
        {
            DisplayLog(" Within the Bounds of a Prefab");
            ScanForTileEntityInList();
            DisplayLog(" Searching for closes container: " + this.lstTileContainers.Count);
            result = FindNearestContainer();
        }

        DisplayLog("CanExecute() End: " + result);
        return result;
    }


    public override bool Continue()
    {
        DisplayLog("CanContinue()");
        bool result = false;
        if (entityAliveSDX)
        {
            result = entityAliveSDX.CanExecuteTask(EntityAliveSDX.Orders.Loot);
            DisplayLog("CanContinue() Loot Task? " + result);
            if (result == false)
                return false;
        }
        
        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
        {
            DisplayLog(" Not Path to continue looting.");
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
                DisplayLog(" Too far away from my investigate Position: " + sqrMagnitude);
                return false;
            }
        }

        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 < 1f || (path != null && path.isFinished()))
        {
            DisplayLog("I'm at the loot container: " + sqrMagnitude2 );
            CheckContainer(); 
            result= FindNearestContainer();
        }
        DisplayLog("Continue() End: " + result);
        return result;
    }

    public bool CheckContainer()
    {
        this.theEntity.SetLookPosition(seekPos);

        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return false; // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return false; // Missed the target. Overlooking?

        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 > 1f)
            return false; // too far away from it

            DisplayLog(" Looking at: " + this.seekPos + " My position is: " + this.theEntity.position);
        TileEntityLootContainer tileEntityLootContainer = this.theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
        if (tileEntityLootContainer == null)
        {
            DisplayLog("No Loot container here.");
            return false;

        }

        GetItemFromContainer(tileEntityLootContainer);
        if (tileEntityLootContainer.IsEmpty())
        {
            DisplayLog(" Looted Container.");
            return true;
        }
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
        DisplayLog("ScanForTileEntityInList()");
        Vector3i blockPosition = this.theEntity.GetBlockPosition();

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
                            {
                                DisplayLog(" This tile Entity has already been touched: " + tileEntities.ToString());
                                continue;
                            }

                            if (Block.list[block.type].HasTag(BlockTags.Door))
                            {
                                DisplayLog(" This tile entity is a door. ignoring.");
                                continue;
                            }
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

        DisplayLog(" Loot List: " + tileLootContainer.lootListIndex);
        if (tileLootContainer.lootListIndex <= 0)
            return;
        if (tileLootContainer.bTouched)
            return;

        tileLootContainer.bTouched = true;
        tileLootContainer.bWasTouched = true;

        DisplayLog("Checking Loot Container");
        if (tileLootContainer.items != null)
        {
            BlockValue block = this.theEntity.world.GetBlock(blockPos);
            String lootContainerName = Localization.Get(Block.list[block.type].GetBlockName(), string.Empty);
            theEntity.SetLookPosition(blockPos.ToVector3());

            DisplayLog(" Loot container is: " + lootContainerName);
            DisplayLog(" Loot Container has this many Slots: " + tileLootContainer.items.Length);

            EntityPlayer player = null;
            if (this.theEntity.Buffs.HasCustomVar("Leader"))
                player = theEntity.world.GetEntity((int)this.theEntity.Buffs.GetCustomVar("Leader")) as EntityPlayerLocal;

            theEntity.MinEventContext.TileEntity = tileLootContainer;
            theEntity.FireEvent(MinEventTypes.onSelfOpenLootContainer);
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState((int)(GameManager.Instance.World.worldTime % 2147483647UL));
            ItemStack[] array = LootContainer.lootList[tileLootContainer.lootListIndex].Spawn(tileLootContainer.items.Length, EffectManager.GetValue(PassiveEffects.LootGamestage, null, (float)player.PartyGameStage, player, null, default(FastTags), true, true, true, true), 0f);
            UnityEngine.Random.state = state;
            for (int i = 0; i < array.Length; i++)
            {
                if (this.theEntity.lootContainer.AddItem(array[i].Clone()))
                {
                    DisplayLog("Removing item from loot container: " + array[i].itemValue.ItemClass.Name);
                }
                else
                {
                    DisplayLog(" Could Not add Item to NPC inventory. " + tileLootContainer.items[i].itemValue.ToString());
                    if (theEntity is EntityAliveSDX)
                    {
                        (theEntity as EntityAliveSDX).ExecuteCMD("Follow", player);
                        return;
                    }

                }

            }
            theEntity.FireEvent(MinEventTypes.onSelfLootContainer);

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

