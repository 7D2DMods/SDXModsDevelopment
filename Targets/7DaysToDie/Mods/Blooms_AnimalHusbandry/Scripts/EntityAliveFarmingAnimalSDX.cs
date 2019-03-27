﻿/*
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

class EntityAliveFarmingAnimalSDX : EntityAliveSDX
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
        // Test Hooks
        DisplayLog(this.ToString());
    }

    // read in the cvar for sizeScale and adjust it based on the buff
    public void AdjustSizeForStage()
    {
        float size = this.Buffs.GetCustomVar("$sizeScale");
        if (size > 0.0f)
            this.gameObject.transform.localScale = new Vector3(size, size, size);
    }


    public override void OnUpdateLive()
    {
        AdjustSizeForStage();
        base.OnUpdateLive();
    }

    //// Farm Animals don't need the complex commands that EntityAliveSDX needs.
    //public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    //{
    //    EntityActivationCommand[] ActivationCommands = new EntityActivationCommand[]
    //    {
    //        new EntityActivationCommand("TellMe", "talk", true )
    //    };
    //    return ActivationCommands;
    //}
    //public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    //{
    //    switch (_indexInBlockActivationCommands)
    //    {
    //        case 0: // Tell me about yourself
    //            GameManager.ShowTooltipWithAlert(_entityFocusing as EntityPlayerLocal, this.ToString() + "\n\n\n\n\n", "ui_denied");
    //            break;
    //    }

    //    return true;
    //}

    public override string ToString()
    {
        String strOutput = base.ToString();

        String strMilkLevel = "0";
        if (this.Buffs.HasCustomVar("MilkLevel"))
        {            
            strMilkLevel = this.Buffs.GetCustomVar("MilkLevel").ToString();
            strOutput += "\n Milk Level: " + strMilkLevel;
        }

        if (this.Buffs.HasCustomVar("$EggValue"))
        {
            String strEggLevel  = this.Buffs.GetCustomVar("$EggValue").ToString();
            strOutput += "\n Egg Level: " + strEggLevel;
        }
        if (this.Buffs.HasCustomVar("Mother"))
        {
            int MotherID = (int)this.Buffs.GetCustomVar("Mother");
            EntityAliveSDX MotherEntity = this.world.GetEntity(MotherID) as EntityAliveSDX;
            if (MotherEntity)
                strOutput += "\n My Mother is: " + MotherEntity.EntityName + " ( " + MotherID + " )";
        }
  
        return strOutput;
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
