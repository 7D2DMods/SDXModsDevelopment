/*
 * Class: EntityAliveSDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive. This is meant to be a base class, where other classes can extend
 *      from, giving them the ability to accept quests and buffs.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features. 
 *
 *      <property name="Class" value="EntityAliveSDX, Mods" />
 */
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

class EntityAliveSDX : EntityAlive
{
    public QuestJournal QuestJournal = new QuestJournal();
    public List<String> lstQuests = new List<String>();
    public Orders currentOrder = Orders.Wander;

    private bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog &&  !this.IsDead())
            Debug.Log(this.entityName + ": " + strMessage);
    }

    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];
        if (entityClass.Properties.Values.ContainsKey("StartingQuests"))
        {
            string text = entityClass.Properties.Values["StartingQuests"];
            foreach (string text2 in text.Split(new char[] {',' }))
            {
                if (this.lstQuests.Contains(text2.Trim()))
                    continue;
                this.lstQuests.Add(text2);
            }
        }

        if (entityClass.Properties.Values.ContainsKey("DebugEntity"))
        {
            this.blDisplayLog = StringParsers.ParseBool(entityClass.Properties.Values["DebugEntity"], 0, -1, true);
        }

    }

    public virtual bool IsEntityDebug()
    {
        return this.blDisplayLog;
    }
    
    public override void PostInit()
    {
        base.PostInit();

        InvokeRepeating("OneMinuteUpdates", 0f, 60f);

    }

    // Reads the buff and quest information
    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        this.Buffs.Read(_br);
        this.QuestJournal = new QuestJournal();
        this.QuestJournal.Read(_br);
        
    }

    // Saves the buff and quest information
    public override void Write(BinaryWriter _bw)
    {
        base.Write(_bw);
        this.Buffs.Write(_bw, true);
        this.QuestJournal.Write(_bw);
    }

    public override string ToString()
    {
        String FoodAmount = ((float)Mathf.RoundToInt(this.Stats.Stamina.ModifiedMax + this.Stats.Entity.Buffs.GetCustomVar("foodAmount"))).ToString() ;
        String WaterAmount = ((float)Mathf.RoundToInt(this.Stats.Water.Value + this.Stats.Entity.Buffs.GetCustomVar("waterAmount"))).ToString();
        string strOutput = this.entityName + " - ID: " + this.entityId + " Health: " + this.Stats.Health.Value + " Stamina: " + this.Stats.Stamina.Value + " Thirst: " + this.Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += "\n Current Order: " + CurrentOrder;
        strOutput += "\n Active Buffs: ";
        foreach (BuffValue buff in this.Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";

        strOutput += "\n Active Quests: ";
        foreach (Quest quest in this.QuestJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;

        return strOutput;
    }

    public void GiveQuest(String strQuest)
    {
        // Don't give duplicate quests.
        foreach (Quest quest in this.QuestJournal.quests)
        {
            if (quest.ID == strQuest.ToLower() )
                return;
        }

        // Make sure the quest is valid
        Quest NewQuest = QuestClass.CreateQuest(strQuest);
        if (NewQuest == null)
            return;

        // If there's no shared owner, it tries to read the PlayerLocal's entity ID. This entity doesn't have that.
        NewQuest.SharedOwnerID = this.entityId;
        NewQuest.QuestGiverID = -1;
        this.QuestJournal.AddQuest(NewQuest);
    }

    protected virtual void SetupStartingItems()
    {
        for (int i = 0; i < this.itemsOnEnterGame.Count; i++)
        {
            ItemStack itemStack = this.itemsOnEnterGame[i];
            ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
            {
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, default(FastTags), 1f);
            }
            else
            {
                itemStack.count = forId.Stacknumber.Value;
            }
            this.inventory.SetItem(i, itemStack);
        }
    }

    // Helper method.
    public bool HasBuff(String strBuff)
    {
        return this.Buffs.ActiveBuffs.Contains(Buffs.GetBuff(strBuff));
    }
   
    public override void OnUpdateLive()
    {
        // Non-player entities don't fire all the buffs, so we'll manually fire the water tick,
        this.Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        this.Stats.UpdateStatsOverTime(0.5f);
      
        base.OnUpdateLive();

        

    }

    protected override void onUnderwaterStateChanged(bool _bUnderwater)
    {
        base.onUnderwaterStateChanged(_bUnderwater);
        if (_bUnderwater)
            this.Buffs.SetCustomVar("_underwater", 1f, true);
        else if (!this.IsDead())
            this.Buffs.SetCustomVar("_underwater", 0f, true);
        else
            this.Buffs.SetCustomVar("_underwater", 0f, true);
    }
    public Orders CurrentOrder
    {
        get
        {
            return this.currentOrder;
        }
        set
        {
            this.currentOrder = value;
        }
    }


    public enum Orders
    {
        Follow = 0,
        Stay = 1,
        Wander = 2,
        Pet = 3
    }
}
