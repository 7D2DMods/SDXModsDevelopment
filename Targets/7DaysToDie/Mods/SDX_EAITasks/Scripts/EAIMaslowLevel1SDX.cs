﻿using GamePath;
using System;
using System.Collections.Generic;
using UnityEngine;

class EAIMaslowLevel1SDX : EAIApproachSpot
{

    List<String> lstFoodItems = new List<String>();
    List<String> lstFoodBins = new List<String>();

    List<String> lstWaterBins = new List<String>();
    List<String> lstHomeBlocks = new List<String>();

    List<String> lstHungryBuffs = new List<String>();
    List<String> lstThirstyBuffs = new List<String>();

    List<String> lstSanitation = new List<String>();
    List<String> lstSanitationBuffs = new List<String>();

    List<String> lstBedTimeBuffs = new List<String>();
    List<String> lstBeds = new List<String>();

    List<ProductionItem> lstProductionItem = new List<ProductionItem>();

    String strSanitationBlock = "";
    String strProductionFinishedBuff = "";

    int MaxDistance = 20;
    public int investigateTicks;
    List<Vector3> lstWaterBlocks = new List<Vector3>();

    public bool hadPath;
    private bool blDisplayLog = true;
    private Vector3 investigatePos;
    private Vector3 seekPos;
    private int pathRecalculateTicks;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }


    public struct ProductionItem
    {
        public ItemValue item;
        public int Count;
        public String cvar;
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        this.MutexBits = 3;
        this.executeDelay = 0.5f;

        // There is too many values that we need to read in from the entity, so we'll read them directly from the entityclass
        EntityClass entityClass = EntityClass.list[_theEntity.entityClass];

        this.lstFoodBins = ConfigureEntityClass("FoodBins", entityClass);
        this.lstFoodItems = ConfigureEntityClass("FoodItems", entityClass);
        this.lstWaterBins = ConfigureEntityClass("WaterBins", entityClass);

        this.lstHomeBlocks = ConfigureEntityClass("HomeBlocks", entityClass);
        this.lstHungryBuffs = ConfigureEntityClass("HungryBuffs", entityClass);
        this.lstThirstyBuffs = ConfigureEntityClass("ThirstyBuffs", entityClass);

        this.lstSanitation = ConfigureEntityClass("ToiletBlocks", entityClass);
        this.lstSanitationBuffs = ConfigureEntityClass("SanitationBuffs", entityClass);

        if (entityClass.Properties.Values.ContainsKey("SanitationBlock"))
            this.strSanitationBlock = entityClass.Properties.Values["SanitationBlock"];

        if (entityClass.Properties.Values.ContainsKey("ProductionFinishedBuff"))
            this.strProductionFinishedBuff = entityClass.Properties.Values["ProductionFinishedBuff"];

        this.lstBeds = ConfigureEntityClass("Beds", entityClass);
        this.lstBedTimeBuffs = ConfigureEntityClass("BedTimeBuffs", entityClass);

        if (entityClass.Properties.Classes.ContainsKey("ProductionItems"))
        {
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["ProductionItems"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                ProductionItem item = new ProductionItem();
                item.item = ItemClass.GetItem(keyValuePair.Key, false);
                item.Count = int.Parse(dynamicProperties3.Values[keyValuePair.Key]);
                

                String strCvar = "Nothing";
                if (dynamicProperties3.Params1.TryGetValue(keyValuePair.Key, out strCvar))
                    item.cvar = strCvar;

                this.lstProductionItem.Add(item);
                DisplayLog("Adding Production Item: " + keyValuePair.Key + " with a count of: " + item.Count + " and will reset: " + strCvar);
            }
        }
    }

    // helper Method to read the entity class and return a list of values based on the key
    // Example: <property name="WaterBins" value="water,waterMoving,waterStaticBucket,waterMovingBucket,terrWaterPOI" />
    public List<String> ConfigureEntityClass(String strKey, EntityClass entityClass)
    {
        List<String> TempList = new List<String>();
        if (entityClass.Properties.Values.ContainsKey(strKey))
        {
            string strTemp = entityClass.Properties.Values[strKey].ToString();
            string[] array = strTemp.Split(new char[]
            {
                ','
            });
            for (int i = 0; i < array.Length; i++)
            {
                if (TempList.Contains(array[i].ToString()))
                    continue;
                TempList.Add(array[i].ToString());
            }

        }
        return TempList;

    }

    // Determines if this AI task can even start. This is based on the thirsty and hunger buffs
    public override bool CanExecute()
    {
        bool result = false;

        if (this.theEntity.IsSleeping)
            return false;

        // If there's no buff incentive, don't execute.
        if (!CheckIncentive(this.lstThirstyBuffs)
            && !CheckIncentive(this.lstHungryBuffs)
            && !CheckIncentive(this.lstSanitationBuffs)
            //&& !CheckIfShelterNeeded()
            )
        {
            this.theEntity.SetInvestigatePosition(Vector3.zero, 0);
            return false;
        }
        if (!this.theEntity.HasInvestigatePosition)
        {
            if (CheckForFoodBin())  // Search for food if its hungry.
                result = true;
            else if (CheckForWaterBlock()) // Search for water.
                result = true;
            else if (CheckForSanitation())  // Potty time?
                result = true;
            else if (CheckForBed())
                result = true;
            else if (CheckForShelter()) // Check for shelder.
                result = true;
            else
            {
                // check and see if you have a home block.
                CheckForHomeBlock();
                this.theEntity.SetInvestigatePosition(Vector3.zero, 0);
                result = false;
            }


        }

        // If We can continue, that means we've triggered the hunger or thirst buff. Investigate Position are set int he CheckFor.. methods
        // If we still don't have a position, then there's no food or water near by that satisfies our needs.
        if (result && this.theEntity.HasInvestigatePosition)
        {
            DisplayLog(" Investigate Pos: " + this.investigatePos + " Current Seek Time: " + investigateTicks + " Max Seek Time: " + this.theEntity.GetInvestigatePositionTicks() + " Seek Position: " + this.seekPos + " Target Block: " + this.theEntity.world.GetBlock(new Vector3i(this.investigatePos)).Block.GetBlockName());
            this.investigatePos = this.theEntity.InvestigatePosition;
            this.seekPos = this.theEntity.world.FindSupportingBlockPos(this.investigatePos);
            return true;
        }
        return false;

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
        lookPosition.y += 0.8f;
        this.theEntity.SetLookPosition(lookPosition);

        // if the entity is blocked by anything, switch it around and tell it to find a new path.
        if (this.theEntity.moveHelper.BlockedTime > 1f)
        {
            this.pathRecalculateTicks = 0;
            this.theEntity.SetLookPosition(lookPosition - Vector3.back);
        }
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
        PathFinderThread.Instance.FindPath(this.theEntity, this.seekPos, this.theEntity.GetMoveSpeed(), false, this);
    }

    // Checks a list of buffs to see if there's an incentive for it to execute.
    public virtual bool CheckIncentive(List<String> Incentives)
    {
        foreach (String strIncentive in Incentives)
        {
            if (this.theEntity.Buffs.HasBuff(strIncentive))
                return true;
        }
        return false;
    }

    public override bool Continue()
    {
        // If there's no buff incentive, or no nearby water block, don't bother looking for water.
        if (!CheckIncentive(this.lstThirstyBuffs) && !CheckIncentive(this.lstHungryBuffs) && !CheckIncentive(this.lstSanitationBuffs) && !CheckIfShelterNeeded())
            return false;

        PathNavigate navigator = this.theEntity.navigator;
        PathEntity path = navigator.getPath();
        if (this.hadPath && path == null)
            return false;

        if (++this.investigateTicks > 40)
        {
            this.investigateTicks = 0;
            if (!this.theEntity.HasInvestigatePosition)
                return false; // no invesitgative position

            float sqrMagnitude = (this.investigatePos - this.theEntity.InvestigatePosition).sqrMagnitude;
            if (sqrMagnitude >= 4f)
            {
                return false; // not close enough.
            }
        }

        float sqrMagnitude2 = (this.seekPos - this.theEntity.position).sqrMagnitude;
        if (sqrMagnitude2 <= 4f || (path != null && path.isFinished()))
        {
            return PerformAction();
        }
        return true;
    }

    // Virtual methods to overload, so we can choose what kind of action to take.
    public virtual bool PerformAction()
    {
        // Look at the target.
        this.theEntity.SetLookPosition(seekPos);

        Ray lookRay = new Ray(this.theEntity.position, theEntity.GetLookVector());
        if (!Voxel.Raycast(this.theEntity.world, lookRay, Constants.cDigAndBuildDistance, -538480645, 4095, 0f))
            return false; // Not seeing the target.

        if (!Voxel.voxelRayHitInfo.bHitValid)
            return false; // Missed the target. Overlooking?

        BlockValue checkBlock = theEntity.world.GetBlock(new Vector3i(seekPos.x, seekPos.y, seekPos.z));

        // Original hand item.
        ItemClass original = this.theEntity.inventory.holdingItem;

        // Execute the drinking process
        if (CheckIncentive(this.lstThirstyBuffs))
        {
            DisplayLog("Thirsty Check Block: " + checkBlock.Block.GetBlockName());
            if (checkBlock.Block.blockMaterial.IsLiquid)
            {
                // Look at the water, then execute the action on the empty jar.
                this.theEntity.SetLookPosition(seekPos);
                if (this.theEntity.inventory.holdingItem.Actions[1] != null)
                {
                    // We want to deplete the water, so execute the action.
                    this.theEntity.inventory.holdingItem.Actions[1].ExecuteAction(this.theEntity.inventory.holdingItemData.actionData[1], true);
                }
            }
            // see if the block is an entity, rather than a watering hold. 
            float milkLevel = this.GetEntityWater();
            if (milkLevel > 0)
            {
                if (this.theEntity.Buffs.HasCustomVar("$Mother"))
                {
                    DisplayLog("Checking For mother");
                    int MotherID = (int)this.theEntity.Buffs.GetCustomVar("$Mother");
                    EntityAliveSDX MotherEntity = this.theEntity.world.GetEntity(MotherID) as EntityAliveSDX;
                    if (MotherEntity)
                    {
                        DisplayLog(" Draining Mommy of milk");
                        MotherEntity.Buffs.SetCustomVar("MilkLevel", 0f, true);
                        this.theEntity.Buffs.SetCustomVar("$foodAmountAdd", 50f, true);
                        this.theEntity.Buffs.SetCustomVar("$waterAmountAdd", 50f, true);
                    }
                }
            }
            // This is the actual item we want to drink out of. The above is just to deplete the water source.
            this.theEntity.inventory.SetBareHandItem(ItemClass.GetItem("drinkJarBoiledWater", false));

            // Then we want to fire off the event on the water we are drinking.
            this.theEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);

            DisplayLog(" Drinking");
            // restore the hand item.
            this.theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));

        }

        if (CheckIncentive(this.lstHungryBuffs))
        {
            DisplayLog("Hunger Check Block: " + checkBlock.Block.GetBlockName());
            if (this.lstFoodBins.Contains(checkBlock.Block.GetBlockName()))
            {
                TileEntityLootContainer tileEntityLootContainer = this.theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
                if (tileEntityLootContainer == null)
                    return false; // it's not a loot container.

                // Check if it has any food on it.
                if (CheckContents(tileEntityLootContainer, this.lstFoodItems))
                {
                    ItemValue item = GetItemFromContainer(tileEntityLootContainer, lstFoodItems);
                    if (item != null)
                    {
                        // Hold the food item.
                        this.theEntity.inventory.SetBareHandItem(item);

                        // We want to consume the food, but the consumption of food isn't supported on the non-players, so just fire off the buff 
                        this.theEntity.FireEvent(MinEventTypes.onSelfPrimaryActionEnd);
                        this.theEntity.FireEvent(MinEventTypes.onSelfHealedSelf);

                        DisplayLog(" Eating");
                        // restore the hand item.
                        this.theEntity.inventory.SetBareHandItem(ItemClass.GetItem(original.Name, false));
                    }
                }
            }
        }

        if (CheckIncentive(this.lstSanitationBuffs))
        {
            if (this.lstSanitation.Contains(checkBlock.Block.GetBlockName()))
                this.theEntity.Buffs.CVars["$solidWasteAmount"] = 0;

            // No toilets.
            if (this.lstSanitation.Count == 0)
            {
                // No Sanitation location? Let it go where you are.
                this.theEntity.Buffs.CVars["$solidWasteAmount"] = 0;

                // if there's no block, don't do anything.
                if (!String.IsNullOrEmpty(strSanitationBlock))
                {
                    Vector3i sanitationBlock = new Vector3i(this.theEntity.position);
                    this.theEntity.world.SetBlockRPC(sanitationBlock, Block.GetBlockValue(this.strSanitationBlock, false));
                }
            }
        }

        if (CheckIncentive(this.lstBedTimeBuffs))
        {
            DisplayLog(" I have the bed time buff.");
            if (this.lstBeds.Contains(checkBlock.Block.GetBlockName()))
            {
                DisplayLog(" My target block is in my approved list. ");
                TileEntityLootContainer tileEntityLootContainer = this.theEntity.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(seekPos)) as TileEntityLootContainer;
                if (tileEntityLootContainer != null)
                {
                    DisplayLog(" It's a TileEntity. That's good.");
                    foreach (ProductionItem item in this.lstProductionItem)
                    {
                        DisplayLog(" Adding " + item.item.GetItemId());
                        // Add the item to the loot container, and reset the cvar, if it's available.
                        tileEntityLootContainer.AddItem(new ItemStack(item.item, item.Count));
                        if (this.theEntity.Buffs.HasBuff(item.cvar))
                            this.theEntity.Buffs.CVars[item.cvar] = 0;
                    }
                }
                else
                    DisplayLog(" Not a tile entity.");
            }
            else
                DisplayLog(" Not an approved block: " + checkBlock.Block.GetBlockName());
        }
        else
        {
            DisplayLog(" No Bed Time buff incentive");
        }


        this.theEntity.SetInvestigatePosition(Vector3.zero, 0);
        return false;
    }


    // Grab a single item from the storage box, and remmove it.
    public ItemValue GetItemFromContainer(TileEntityLootContainer tileLootContainer, List<String> lstContents)
    {
        if (tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.items;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty()) // nothing in the slot
                    continue;

                // The animals will only eat the food they like best.
                if (lstContents.Contains(array[i].itemValue.ItemClass.Name))
                {
                    // if there's only one left, remove the entire item; otherwise, decrease it.
                    if (array[i].count == 1)
                        tileLootContainer.RemoveItem(array[i].itemValue);
                    else
                        array[i].count--;

                    return array[i].itemValue;
                }
            }
        }
        return null;
    }

    // This will check if the food item actually exists in the container, before making the trip to it.
    public bool CheckContents(TileEntityLootContainer tileLootContainer, List<String> lstContents)
    {
        DisplayLog(" Check Contents of Food bin");
        DisplayLog(" TileEntity: " + tileLootContainer.items.Length);
        DisplayLog(" Food Items: " + lstContents.Count);
        if (tileLootContainer.items != null && lstContents != null)
        {
            ItemStack[] array = tileLootContainer.items;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty())
                    continue;

                // The animals will only eat the food they like best.
                if (lstContents.Contains(array[i].itemValue.ItemClass.Name))
                    return true;
            }
        }

        return false;
    }

    // Check if the entity needs to poop, and where it should go.
    public virtual bool CheckForSanitation()
    {
        if (!CheckIncentive(this.lstSanitationBuffs))
            return false;

        if (this.lstSanitation.Count > 0)
        {
            Vector3 TargetBlock = ScanForTileEntityInList(this.lstSanitation, new List<string>());
            if (TargetBlock == Vector3.zero)
                return false;

            this.theEntity.SetInvestigatePosition(TargetBlock, 120);
        }
        return true;
    }

    public virtual bool CheckForBed()
    {
        if (!CheckIncentive(this.lstBedTimeBuffs))
            return false;

        // If it's an egg producing entity, scan for a bed to hatch.
        if (this.theEntity.Buffs.HasBuff(strProductionFinishedBuff))
        {
            Vector3 TargetBlock = ScanForTileEntityInList(this.lstBeds, new List<String>());
            if (TargetBlock != Vector3.zero)
            {
                this.theEntity.SetInvestigatePosition(TargetBlock, 120);
                return true;
            }
        }
        return false;
    }
    // Check if the entity is hungry and if there's a food bin nearby.
    public virtual bool CheckForFoodBin()
    {
        if (!CheckIncentive(this.lstHungryBuffs))
            return false;

        Vector3 TargetBlock = ScanForTileEntityInList(this.lstFoodBins, this.lstFoodItems);
        if (TargetBlock == Vector3.zero)
            return false;

        this.theEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    public virtual Vector3 ScanForTileEntityInList(List<String> lstBlocks, List<String> lstContents)
    {
        // If there's no blocks to look for, don't do anything.
        if (lstBlocks.Count == 0)
            return Vector3.zero;

        List<Vector3> localLists = new List<Vector3>();


        // Otherwise, search for your new home.
        Vector3i blockPosition = this.theEntity.GetBlockPosition();
        int num = World.toChunkXZ(blockPosition.x);
        int num2 = World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (Chunk)theEntity.world.GetChunkSync(num + j, num2 + i);
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

                            // if its not a listed block, then keep searching.
                            if (!lstBlocks.Contains(block.Block.GetBlockName()))
                                continue;


                            if (lstContents.Count > 0)
                            {
                                if (CheckContents(tileEntity, lstContents))
                                {
                                    DisplayLog(" Box has food contents: " + tileEntities.ToString());
                                    localLists.Add(tileEntity.ToWorldPos().ToVector3());

                                }
                                else
                                    DisplayLog(" Empty Container: " + tileEntities.ToString());
                            }
                            else
                                localLists.Add(tileEntity.ToWorldPos().ToVector3());

                        }
                    }
                }
            }
        }

        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (Vector3 block in localLists)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        return tMin;
    }

    // This will search for a mother entity to see it can satisfy its thirst from its mother, rather than a traditional water block.
    public virtual float GetEntityWater()
    {
        if (this.theEntity.Buffs.HasCustomVar("$Mother"))
        {
            float MotherID = this.theEntity.Buffs.GetCustomVar("$Mother");
            EntityAliveSDX MotherEntity = this.theEntity.world.GetEntity((int)MotherID) as EntityAliveSDX;
            if (MotherEntity)
            {
                DisplayLog(" My Mother is: " + MotherEntity.EntityName);
                if (MotherEntity.Buffs.HasCustomVar("MilkLevel"))
                {
                    DisplayLog("Heading to mommy");
                    float MilkLevel = MotherEntity.Buffs.GetCustomVar("MilkLevel");
                    this.theEntity.SetInvestigatePosition(this.theEntity.world.GetEntity((int)MotherID).position, 60);
                    return MilkLevel;
                }
            }

        }
        return 0f;
    }

    // Scans for the water block in the area.
    public virtual bool CheckForWaterBlock()
    {
        if (!CheckIncentive(this.lstThirstyBuffs))
            return false;

        // This check is if we are a baby, and are seeking the mother to satisfy thirst.
        if (GetEntityWater() > 0f)
            return true;

        Vector3 TargetBlock = ScanForBlockInList(this.lstWaterBins);
        if (TargetBlock == Vector3.zero)
            return false;

        this.theEntity.SetInvestigatePosition(TargetBlock, 120);
        return true;
    }

    // Scans for the water block in the area.
    public virtual bool CheckForHomeBlock()
    {
        if (this.lstHomeBlocks.Count == 0)
            return false;

        Vector3 TargetBlock = ScanForBlockInList(this.lstHomeBlocks);
        if (TargetBlock == Vector3.zero)
            return false;

        Vector3i position;
        position.x = Utils.Fastfloor(TargetBlock.x);
        position.z = Utils.Fastfloor(TargetBlock.z);
        position.y = Utils.Fastfloor(TargetBlock.y);
        this.theEntity.setHomeArea(position, this.MaxDistance);
        return true;
    }
    // The method will scan a distance of MaxDistance around the entity, finding the nearest block that matches in the list.
    public virtual Vector3 ScanForBlockInList(List<String> lstBlocks)
    {
        if (lstBlocks.Count == 0)
            return Vector3.zero;

        if (!CheckIncentive(this.lstThirstyBuffs))
            return Vector3.zero;

        List<Vector3> localLists = new List<Vector3>();

        Vector3i blockPosition = theEntity.GetBlockPosition();
        Vector3i TargetBlockPosition = new Vector3i();

        for (var x = (int)blockPosition.x - this.MaxDistance; x <= blockPosition.x + this.MaxDistance; x++)
        {
            for (var z = (int)blockPosition.z - this.MaxDistance; z <= blockPosition.z + this.MaxDistance; z++)
            {
                for (var y = (int)blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    TargetBlockPosition.x = x;
                    TargetBlockPosition.y = y;
                    TargetBlockPosition.z = z;

                    BlockValue block = theEntity.world.GetBlock(TargetBlockPosition);
                    // if its not a listed block, then keep searching.
                    if (!lstBlocks.Contains(block.Block.GetBlockName()))
                        continue;

                    localLists.Add(TargetBlockPosition.ToVector3());
                }
            }
        }

        // Finds the closet block we matched with.
        Vector3 TargetBlock = new Vector3();
        TargetBlock = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (Vector3 block in localLists)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                TargetBlock = block;
                minDist = dist;
            }
        }

        return TargetBlock;
    }


    public virtual bool IsSheltered(Vector3i position)
    {
        // We only want to check air positions.
        if (this.theEntity.world.GetBlock(position).type != 0)
            return false;

        float num = 1f;
        int x = position.x;
        int y = position.y;
        int z = position.z;
        IChunk chunkFromWorldPos = this.theEntity.world.GetChunkFromWorldPos(x, y, z);

        num = (float)Mathf.Max((int)chunkFromWorldPos.GetLight(x, y, z, Chunk.LIGHT_TYPE.SUN), (int)chunkFromWorldPos.GetLight(x, y + 1, z, Chunk.LIGHT_TYPE.SUN));
        num /= 15f;
        return (1f - num > 0.3f);

    }

    public virtual bool CheckIfShelterNeeded()
    {

        return false;
        bool results = false;

        // Night time, go hide!
        if (this.theEntity.world.IsDaytime() == false)
            results = true;

        // if the entity is already sheltered, then no need to go anywhere.
        if (this.theEntity.Buffs.HasCustomVar("_sheltered"))
            if (this.theEntity.Buffs.GetCustomVar("_sheltered") < WeatherParams.EnclosureDetectionThreshold) // below sheltered level.
                results = true;
            else
                return false;



        return results;
    }

    public virtual bool CheckForShelter()
    {
        if (!CheckIfShelterNeeded())
            return false;

        DisplayLog(" Shelter is needed: ");
        List<Vector3> ShelteredBlocks = new List<Vector3>();

        Vector3i blockPosition = World.worldToBlockPos(this.theEntity.GetPosition());
        IChunk chunkFromWorldPos = this.theEntity.world.GetChunkFromWorldPos(blockPosition);
        Vector3i currentBlock = new Vector3i();

        for (var x = (int)blockPosition.x - this.MaxDistance; x <= blockPosition.x + this.MaxDistance; x++)
        {
            for (var z = (int)blockPosition.z - this.MaxDistance; z <= blockPosition.z + this.MaxDistance; z++)
            {
                for (var y = (int)blockPosition.y - 5; y <= blockPosition.y + 5; y++)
                {
                    currentBlock.x = x;
                    currentBlock.y = y;
                    currentBlock.z = z;
                    if (chunkFromWorldPos != null && y >= 0 && y < 255)
                    {
                        // Before adding the sheltered block to a valid list, let's make sure it's sheltered all around it.
                        // It's air checked, we don't really care about the other blocks around it.
                        if (IsSheltered(currentBlock)
                            && IsSheltered(currentBlock + Vector3i.back)
                            && IsSheltered(currentBlock + Vector3i.forward)
                            && IsSheltered(currentBlock + Vector3i.left)
                            && IsSheltered(currentBlock + Vector3i.right)
                            && IsSheltered(currentBlock + Vector3i.up))
                        {
                            ShelteredBlocks.Add(currentBlock.ToVector3());
                        }
                    }
                }
            }
        }



        // Finds the closet block we matched with.
        Vector3 tMin = new Vector3();
        tMin = Vector3.zero;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = this.theEntity.position;
        foreach (Vector3 block in ShelteredBlocks)
        {
            float dist = Vector3.Distance(block, currentPos);
            if (dist < minDist)
            {
                tMin = block;
                minDist = dist;
            }
        }

        if (tMin != Vector3.zero)
        {
            DisplayLog(" Close shelter is: " + tMin.ToString());
            this.theEntity.SetInvestigatePosition(tMin, 100);
            this.theEntity.setHomeArea(new Vector3i(tMin), 50);
            return true;
        }

        return false;
    }

}

