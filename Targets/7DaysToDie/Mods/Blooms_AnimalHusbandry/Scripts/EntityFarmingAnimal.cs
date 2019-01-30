/*
 * Class: EntityFarmingAnimal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base EntityNPC, but allows them to wander around, and are vulnerable to attack.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityWanderingTrader, Mods" />
 */
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

class EntityFarmingAnimal : EntityAlive
{
    public String strFoodItem = "";
    public String strProductItem = "";
    public String strHarvestItems = "";
    public String strHomeBlock = "";
    private float HarvestDelay = 10f;

    // how far the animal will wander from its Home position.
    private int MaxDistanceFromHome = 15;
    private float MaxDistanceToSeePlayer = 20f;

    // Introduce some check delays to reduce the amount of calls per tick on them
    float CheckDelay = 5f;
    float LongerDelay = 3600f;
    float nextLongerCheck = 0;

    private float nextCheck = 0;
    private float ProductDelay = 2400f;
    private float productNextCheck = 0;
    private bool blDisplayLog = true;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.entityName + ": " + strMessage);
    }

    
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

    }

    public void DisplayEntityStats()
    {
        String strMessage = "Entity ID: " + this.entityId;
        strMessage += " Current Health: " + this.Stats.Health.ValuePercent * 100;
        strMessage += " Current Stamina: " + this.Stats.Stamina.ModifiedMaxPercent;
        strMessage += " modifiedMax " + this.Stats.Stamina.ModifiedMax;
        strMessage += " FoodAmount: " + this.GetCVar("$foodAmount");
        DisplayLog(strMessage);

        if (this.Buffs.ActiveBuffs.Count == 0)
        {
            DisplayLog("Adding Buff");
            this.Buffs.AddBuff("buffAnimalStatusCheck", -1, true);
        }

        foreach( var buff in this.Buffs.ActiveBuffs)
        {
            DisplayLog("Active Buff: " + buff.BuffName);
        }
    }


    public override void OnUpdateLive()
    {
        base.OnUpdateLive();

        this.Stats.Water.Tick(0.05f, this.world.worldTime, false);
        this.Stats.Health.Tick(0.05f, this.world.worldTime, false);
        this.Stats.Stamina.Tick(0.05f, this.world.worldTime, false);

        // We only want to do this check periodically to avoid unnecessary overhad.
        if (nextLongerCheck < Time.time)
        {
            nextLongerCheck = Time.time + LongerDelay;
            FindHomePosition();
        }

        // Quicker update to check if there's any entities around
        if (CheckDelay < Time.time)
        {
            
            
            DisplayEntityStats();
            // If the entity can see a player, and the player is holding its food item, start moving towards the player.
            List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * MaxDistanceToSeePlayer));
            if (entitiesInBounds.Count > 0)
            {
                for (int i = 0; i < entitiesInBounds.Count; i++)
                {
                    if (entitiesInBounds[i] is EntityPlayer)
                    {
                        EntityPlayer entityPlayer = entitiesInBounds[i] as EntityPlayer;
                        DisplayLog("Close Player: " + entityPlayer.name);
                        DisplayLog("Player is holding: " + entityPlayer.inventory.holdingItem.Name);
                        if (entityPlayer.inventory.holdingItem.Name == strFoodItem)
                        {
                            DisplayLog(" Player is holding Food! Move towards the player");
                            SetInvestigatePosition(entityPlayer.GetBlockPosition().ToVector3(), 300);
                        }

                    }
                }

            }

        }
    }

    // If the animal has a home block set, then look around for it.
    private void FindHomePosition()
    {
        if (String.IsNullOrEmpty(this.strHomeBlock))
            return;
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
                            DisplayLog("Tile Entity is a Loot Container");
                            BlockValue block = this.world.GetBlock(tileEntity.ToWorldPos());

                            // If it's not the entities home block, move to the next one.
                            if (block.Block.GetBlockName() != this.strHomeBlock)
                                continue;

                            Block block2 = Block.list[block.type];
                            if (block2.RadiusEffects != null)
                            {
                                DisplayLog("Block has a Radius Effect");
                                float distanceSq = base.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                {
                                    
                                    BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                    if (blockRadiusEffect.variable == "buffFoodTrough")
                                    {

                                        if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius)
                                        {
                                            this.setHomeArea(tileEntity.ToWorldPos(), MaxDistanceFromHome);
                                            DisplayLog("Setting Home Position: " + this.getHomePosition());
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
    }

    public void HarvestWithTimer( EntityAlive _player)
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
        if (this.IsDead())
            return new EntityActivationCommand[0];

        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Pet", "hand", true),
            new EntityActivationCommand("Milk", "hand", true)

        };
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        if ( _indexInBlockActivationCommands == 0 )
        {
            // Make happy sound
        }
        else if ( _indexInBlockActivationCommands == 1 )
        {
            // Grab a reference to whatever item the player is holding
            String strHoldingItem = _entityFocusing.inventory.holdingItem.Name;
            DisplayLog("OnEntityActivated: Holding Item: " + strHoldingItem);

            // If it's not in the list of valid items, don't do anything.
            if (!this.strHarvestItems.Contains(strHoldingItem))
                return false;

            if (productNextCheck < Time.time)
            {
                productNextCheck = Time.time + ProductDelay;
                HarvestWithTimer(_entityFocusing);
            }
        }

        return true;
    }


}
