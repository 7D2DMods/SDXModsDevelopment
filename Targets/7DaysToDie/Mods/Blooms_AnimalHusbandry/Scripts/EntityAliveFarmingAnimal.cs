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

    protected override void Awake()
    {
        //BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        //if (component == null)
        //    component = base.gameObject.AddComponent<BoxCollider>();
        //if (component)
        //{
        //    component.center = new Vector3(0f, 0.85f, 0f);
        //    component.size = new Vector3(2f, 1.6f, 2f);
        //}
        base.Awake();
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
         // Test Hooks
        DisplayLog(this.ToString());
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


    public override void OnUpdateLive()
    {
        AdjustSizeForStage();
    
            base.OnUpdateLive();
    }


    public override string ToString()
    {
        String strOutput = base.ToString();

        String strMilkLevel = "0";
        String strMother = "None";
        if (this.Buffs.HasCustomVar("MilkLevel"))
            strMilkLevel = this.Buffs.GetCustomVar("MilkLevel").ToString();

        if (this.Buffs.HasCustomVar("$Mother"))
             strMother = this.Buffs.GetCustomVar("$Mother").ToString();

        strOutput += "\n Milk Level: " + strMilkLevel;
        strOutput += "\n My Mother is: " + strMother;
        return strOutput;
       
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
