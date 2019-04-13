﻿/*
 * Class: EntityAliveFarmingAnimal
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

    /*
     *  Before BoundaryBox: Center: (0.0, 0.9, 0.0), Extents: (0.3, 0.9, 0.2)
Box Collider: (0.6, 1.8, 0.4)
 Box Center: (0.0, 0.9, 0.0)
 After BoundaryBox: Center: (0.0, 0.0, 0.0), Extents: (2.5, 1.0, 2.5)
 */

    protected override void Awake()
    {
        base.Awake();

        Debug.Log(" Before BoundaryBox: " + this.boundingBox.ToCultureInvariantString());
        foreach (BoxCollider box in base.gameObject.GetComponentsInChildren<BoxCollider>())
        {
            Debug.Log("Box Collider: " + box.size.ToCultureInvariantString());
            Debug.Log(" Box Center: " + box.center.ToCultureInvariantString());
        }
        BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        if (component)
        {
            // Re-adjusting the box collider     
            //component.center = new Vector3(0f, 0.85f, 0f);
            float x = component.size.x * 6;
            float z = component.size.z * 6;
            component.size = new Vector3(x,  component.size.y, z);

            this.nativeCollider = component;
            this.scaledExtent = component.size;
            this.boundingBox = BoundsUtils.BoundsForMinMax(x, component.size.y, z, x, component.size.y, z); 
            if (this.isDetailedHeadBodyColliders())
            {
                component.enabled = false;
            }
            Debug.Log(" After BoundaryBox: " + this.boundingBox.ToCultureInvariantString());
        }
    }
    public override void CopyPropertiesFromEntityClass()
    {
        this.npcID = "animalFarm";
        
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
