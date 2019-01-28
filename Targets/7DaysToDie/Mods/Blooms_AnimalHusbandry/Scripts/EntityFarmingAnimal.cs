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

class EntityFarmingAnimal : EntityAnimalStag
{
    public String strFoodItem = "resourceYuccaFibers";
    public String strProductItem = "water";
    public String strHarvestItems = "drinkJarEmptyMilk,drinkJarEmptyMilk";
    float CheckDelay = 5f;
    private float nextCheck = 0;
    private float ProductDelay = 2400f;
    private float productNextCheck = 0;
    private bool blDisplayLog = true;

    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
        {
            Debug.Log(this.entityName + ": " + strMessage);
        }
    }
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];
        if (entityClass.Properties.Values.ContainsKey("Food"))
            strFoodItem = entityClass.Properties.Values["Food"];
        if (entityClass.Properties.Values.ContainsKey("Product"))
            strProductItem = entityClass.Properties.Values["Product"];
        if (entityClass.Properties.Values.ContainsKey("HarvestItems"))
            strHarvestItems = entityClass.Properties.Values["HarvestItems"];
    }
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();
        // We only want to do this check periodically to avoid unnecessary overhad.
        if (nextCheck < Time.time)
        {
            DisplayLog("OnUpdateLive() Checking Bounds");
            nextCheck = Time.time + CheckDelay;
            updateBlockRadiusEffects();

            // If the entity can see a player, and the player is holding its food item, start moving towards the player.
            List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 20f));
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
                            DisplayLog(" Player is holding Food!");
                            SetInvestigatePosition(entityPlayer.GetBlockPosition().ToVector3(), 300);
                        }

                    }
                }

            }

        }
    }

    private void updateBlockRadiusEffects()
    {
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
                            Block block2 = Block.list[block.type];
                            if (block2.RadiusEffects != null)
                            {
                                DisplayLog("Block has a Radius Effect");
                                float distanceSq = base.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());
                                for (int l = 0; l < block2.RadiusEffects.Length; l++)
                                {
                                    BlockRadiusEffect blockRadiusEffect = block2.RadiusEffects[l];
                                    if (distanceSq <= blockRadiusEffect.radius * blockRadiusEffect.radius)
                                    {
                                        
                                        this.setHomeArea(tileEntity.ToWorldPos(), 15);
                                        DisplayLog("Setting Home Position: " + this.getHomePosition() );

                                        DisplayLog(" Close Enough To Feed Trough");
                                        // If the feeding bin has food, reset the despawn time.
                                        if (CheckFoodBox((TileEntityLootContainer)tileEntity))
                                            ResetDespawnTime();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


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

                if (array[i].itemValue.ItemClass.Name == this.strFoodItem)
                {
                    DisplayLog(" Feeding Trough has food");
                    if (array[i].count == 1)
                    {
                        tileLootContainer.RemoveItem(array[i].itemValue);
                        DisplayLog("Consumed a food.");
                    }
                    else
                    {
                        DisplayLog("Decrementing Food by 1");
                        array[i].count--;
                        //  tileLootContainer.UpdateSlot(i, array[i].Clone());

                    }
                    return true;
                }
            }
        }

        return true;
    }

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        if (this.IsDead())
        {
            return new EntityActivationCommand[0];
        }
        return new EntityActivationCommand[]
        {
            new EntityActivationCommand("Pet", "hand", true),
            new EntityActivationCommand("Milk", "search", true)
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

            // If it's not in the list of valid items, don't do anything.
            if (!this.strHarvestItems.Contains(strHoldingItem))
                return false;

            DisplayLog("OnEntityActivated: Holding Item: " + strHoldingItem);

            if (productNextCheck < Time.time)
            {
                productNextCheck = Time.time + ProductDelay;
                ItemValue item = ItemClass.GetItem(this.strProductItem, false);
                _entityFocusing.inventory.SetItem(_entityFocusing.inventory.holdingItemIdx, new ItemStack(item, _entityFocusing.inventory.holdingCount));
            }
        }

        return true;
    }


}
