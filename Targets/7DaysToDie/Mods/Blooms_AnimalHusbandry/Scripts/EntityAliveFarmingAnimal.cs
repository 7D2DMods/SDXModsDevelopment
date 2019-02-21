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


        InvokeRepeating("CheckAnimalEvent", 1f, 60f);
    }

    // Cows were being stuck on the fence and trying to attack them. This is, I think, due to the entity move helper which makes
    // it attack blocks that get in its way, ala zombie.
    public override bool Attack(bool _bAttackReleased)
    {
        if (this.attackTarget == null)
            return false;

        return base.Attack(_bAttackReleased);
    }

    public void CheckAnimalEvent()
    {
        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        int hour = GameUtils.WorldTimeToHours(GameManager.Instance.World.GetWorldTime());
        int minute = GameUtils.WorldTimeToMinutes(GameManager.Instance.World.GetWorldTime());

        // Look for a new home position buff. It bails early if it already has a home buff
        // FindHomePosition();

        //foreach (Mod myMod in ModManager.GetLoadedMods())
        //    Debug.Log(myMod.ModInfo.Name + " by " + myMod.ModInfo.Author + " - " + myMod.ModInfo.Description);


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


        EntityActivationCommand[] ActivationCommands = new EntityActivationCommand[]
        {
            new EntityActivationCommand("Tell me about yourself", "talk", true ),
            new EntityActivationCommand("Pet", "hand", true ),
            new EntityActivationCommand("Follow Me", "talk", true),
            new EntityActivationCommand("Stay here", "talk", true),
            new EntityActivationCommand("Hang out here", "talk", true)
        };

        return ActivationCommands;
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        DisplayLog("OnEntityActivated");
        switch (_indexInBlockActivationCommands)
        {
            case 0:
                GameManager.ShowTooltipWithAlert(_entityFocusing as EntityPlayerLocal, this.ToString() + "\n\n\n\n\n", "ui_denied");
                break;
            case 1:
                // Pet
                break;
            case 2:
                this.CurrentOrder = Orders.Follow;
                break;
            case 3:
                this.CurrentOrder = Orders.Stay;
                break;
            case 4:
                this.CurrentOrder = Orders.Wander;
                break;
            default:
                break;
        }

        return true;
    }

    public override bool CanEntityJump()
    {
        return false;
    }
    public override bool Jumping
    {
        get
        {
            return false;
        }
        set
        {

        }
    }
}
