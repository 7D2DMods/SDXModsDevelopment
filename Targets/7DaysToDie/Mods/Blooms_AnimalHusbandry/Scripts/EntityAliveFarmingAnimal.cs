/*
 * Class: EntityAliveSDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base EntityAliveSDX, and allows animal husbandry... breeding, etc
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityAliveFarmingAnimal, Mods" />
 */
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

class EntityAliveFarmingAnimal : EntityAliveSDX
{
    public String strFoodItem;
    public String strProductItem;
    public String strHarvestItems;
    public String strHomeBlock;
    public String strHomeBuff;

    // how far the animal will wander from its Home position.
    public int MaxDistanceFromHome = 15;
    public float MaxDistanceToSeePlayer = 20f;
    public float HarvestDelay = 10f;

    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];

        if (entityClass.Properties.Values.ContainsKey("FoodItem"))
            this.strFoodItem = entityClass.Properties.Values["FoodItem"];
        if (entityClass.Properties.Values.ContainsKey("ProductItem"))
            this.strProductItem = entityClass.Properties.Values["ProductItem"];
        if (entityClass.Properties.Values.ContainsKey("HarvestItems"))
            this.strHarvestItems = entityClass.Properties.Values["HarvestItems"];
        if (entityClass.Properties.Values.ContainsKey("HomeBlock"))
            this.strHomeBlock = entityClass.Properties.Values["HomeBlock"];
        if (entityClass.Properties.Values.ContainsKey("HomeBuff"))
            this.strHomeBuff = entityClass.Properties.Values["HomeBuff"];

    }

    public void CheckAnimalEvent()
    {
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
        int minute = GameUtils.WorldTimeToMinutes(GameManager.Instance.World.GetWorldTime());

        // Look for a new home position buff. It bails early if it already has a home buff
       // FindHomePosition();

        // Check the size scale for the entity 
        AdjustSizeForStage();

        // Test Hooks
        DisplayLog(this.ToString());


        if (day % 7 == 0) // Blood Moon Day Events
        {
            // Become a bit restless
            switch (hour)
            {
                case 19:
                    // Get nervous
                    break;
                case 20:
                    // Get more nervous
                    break;
                case 21:
                    // Get panicky
                    break;
                case 22:
                    // Freak out OMG
                    break;
            }
            return;  // Do not process any subsequent rules. No milking, no harvesting; animals are too upset.
        }



        switch (hour)
        {
 
            case 21:
                // Nearly night time event
            //    FindPotentialFather();
                break;
            case 22:
                // night time
                break;
        }

 
    }

    // read in the cvar for sizeScale and adjust it based on the buff
    public void AdjustSizeForStage()
    {
        float size = this.Buffs.GetCustomVar("$sizeScale");
        if (size > 0.0f)
        {
            this.gameObject.transform.localScale = new Vector3(size, size, size);
        }
    }
   
 
    //private void FindPotentialFather()
    //{
    //    if (this.IsMale || this.strMaleEntity == "")
    //        return;

    //    // Already pregnant? Brand New Mother? Not An Adult?
    //    if (HasBuff(strPregnancyBuff) || HasBuff("buffNewMother") || !HasBuff(this.strAdultBuff))
    //        return;

    //    List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 4)); // 4 blocks away scan for daddy
    //    if (entitiesInBounds.Count > 0)
    //    {
    //        for (int i = 0; i < entitiesInBounds.Count; i++)
    //        {
    //            if (entitiesInBounds[i] is EntityAliveSDX)
    //            {
    //                // If the Farm Animal is male and the same type, then allow them to breed
    //                EntityAliveSDX entityAnimal = entitiesInBounds[i] as EntityAliveSDX;
    //                if (entityAnimal.IsMale && entityAnimal.entityName == this.strMaleEntity)
    //                    this.Buffs.AddBuff(strPregnancyBuff, -1, true);
    //            }
    //        }

    //    }

    //}
    // If the animal has a home block set, then look around for it.
    private void FindHomePosition()
    {
        if (String.IsNullOrEmpty(this.strHomeBlock))
            return;

        // Check to see if this is already set correctly.
        if (strHomeBlock == this.world.GetBlock(this.getHomePosition().position).Block.GetBlockName())
            return;

        DisplayLog("Searching for Home Block...");
        // Otherwise, search for your new home.
        Vector3i blockPosition = base.GetBlockPosition();
        int num = global::World.toChunkXZ(blockPosition.x);
        int num2 = global::World.toChunkXZ(blockPosition.z);
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                Chunk chunk = (global::Chunk)this.world.GetChunkSync(num + j, num2 + i);
                if (chunk != null)
                {
                    global::DictionaryList<global::Vector3i, global::TileEntity> tileEntities = chunk.GetTileEntities();
                    for (int k = 0; k < tileEntities.list.Count; k++)
                    {
                        TileEntityLootContainer tileEntity = tileEntities.list[k] as TileEntityLootContainer;
                        if (tileEntity != null)
                        {
                            BlockValue block = this.world.GetBlock(tileEntity.ToWorldPos());
                            DisplayLog(" Found TileEntity: " + block.Block.GetBlockName());
                            // If it's not the entities home block, move to the next one.
                            if (block.Block.GetBlockName() != this.strHomeBlock)
                                continue;

                            DisplayLog("Found a home Block: " + block.Block.GetBlockName());
                            Block block2 = Block.list[block.type];
                            if (block2.RadiusEffects != null)
                            {
                                float distanceSq = base.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                {
                                    BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                    DisplayLog(" RadiusEffect: " + blockRadiusEffect.variable + " The Buff: " + this.strHomeBuff);
                                    if (blockRadiusEffect.variable == strHomeBuff)
                                    {
                                        if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius)
                                        {
                                            this.setHomeArea(tileEntity.ToWorldPos(), MaxDistanceFromHome);
                                            DisplayLog("Setting Home Position: " + this.getHomePosition().ToString());
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

    private void EventData_Event(object obj)
    {
        object[] array = (object[])obj;

        // bring up the player's UI to show the timer.
        EntityPlayerLocal entityPlayerLocal = array[0] as EntityPlayerLocal;
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);

        ItemStack itemStack = new ItemStack(ItemClass.GetItem(this.strProductItem, false), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
        {
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        }

        // remove the harvesting item from the player
        entityPlayerLocal.inventory.DecItem(ItemClass.GetItem(entityPlayerLocal.inventory.holdingItem.Name, false), 1);

        // Remove the product from the animal inventory.
        this.inventory.DecItem(ItemClass.GetItem(this.strProductItem, false), 1);
    }

    public void HarvestWithTimer(EntityAlive _player)
    {
        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true, false, true);
        XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
        TimerEventData timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _player
        };
        timerEventData.Event += this.EventData_Event;
        childByType.SetTimer(this.HarvestDelay, timerEventData);
    }

    public bool CheckFoodBox(TileEntityLootContainer tileLootContainer)
    {
        if (tileLootContainer.items != null)
        {
            ItemStack[] array = tileLootContainer.items;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].IsEmpty())
                    continue;

                // The animals will only eat the food they like best.
                if (array[i].itemValue.ItemClass.Name == this.strFoodItem)
                {
                    DisplayLog(" Feeding Trough has food");
                    DisplayLog("Consuming food.");

                    // if there's only one left, remove the entire item; otherwise, decrease it.
                    if (array[i].count == 1)
                        tileLootContainer.RemoveItem(array[i].itemValue);
                    else
                        array[i].count--;
                    return true;
                }
            }
        }

        return false;
    }

    
    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Pet", "hand", true),
            new EntityActivationCommand("Milk", "hand", true)

        };
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        DisplayLog("OnEntityActivated");
        if (_indexInBlockActivationCommands == 0)
        {
            String strDisplay = "Cow Betsy: Food: " + this.Stats.Stamina.Value + " Water: " + this.Stats.Water.Value;
            GameManager.ShowTooltipWithAlert(_entityFocusing as EntityPlayerLocal, this.ToString(), "ui_denied");
            return false;
            // Make happy sound
        }
        else if (_indexInBlockActivationCommands == 1)
        {
            // Grab a reference to whatever item the player is holding
            String strHoldingItem = _entityFocusing.inventory.holdingItem.Name;
            DisplayLog("OnEntityActivated: Holding Item: " + strHoldingItem);

            // If it's not in the list of valid items, don't do anything.
            if (!this.strHarvestItems.Contains(strHoldingItem))
                return false;

            // Check to see if the entity has something to harvest it, and return it.
            ItemValue newItem = ItemClass.GetItem(this.strProductItem, false);
            if (this.inventory.GetItemCount(newItem, false, -1, -1) > 1)
                HarvestWithTimer(_entityFocusing);

        }

        return true;
    }


}
