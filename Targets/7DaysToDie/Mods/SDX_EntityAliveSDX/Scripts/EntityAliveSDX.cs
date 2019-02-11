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

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
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
    }

    public override void PostInit()
    {
        base.PostInit();
        foreach (String strQuest in this.lstQuests)
            GiveQuest(strQuest);
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
        string strOutput = this.entityName + " - ID: " + this.entityId + " Health: " + this.Health + " Hunger: " + this.Stats.Stamina.Value + " Thirst: " + this.Stats.Water.Value;
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
            if (quest.ID == strQuest)
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

    // Helper method.
    public bool HasBuff(String strBuff)
    {
        return this.Buffs.ActiveBuffs.Contains(Buffs.GetBuff(strBuff));
    }

    public override void OnUpdateLive()
    {
        // Update it's entity stats.
        this.Stats.UpdateStatsOverTime(0.05f);
        base.OnUpdateLive();
    }
}
