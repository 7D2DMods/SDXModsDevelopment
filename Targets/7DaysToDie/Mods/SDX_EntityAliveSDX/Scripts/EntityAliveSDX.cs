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
    public List<Vector3> PatrolCoordinates = new List<Vector3>();

    String strMyName = "Bob";
    public System.Random random = new System.Random();

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog &&  !this.IsDead())
            Debug.Log(this.entityName + ": " + strMessage);
    }

    // These are the orders, used in cvars for the EAI Tasks. They are casted as floats.
    public enum Orders
    {
        Follow = 0,
        Stay = 1,
        Wander = 2,
        Pet = 3,
        SetPatrolPoint = 4,
        EndPatrolPoint = 5,
        Patrol = 6
    }


    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];

        // Read in a list of names then pick one at random.
        if (entityClass.Properties.Values.ContainsKey("Names"))
        {
            string text = entityClass.Properties.Values["Names"];
            string[] Names = text.Split(',');
            int index = random.Next(0, Names.Length);
            strMyName = Names[index];
        }

    }

    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing)
    {
        EntityActivationCommand[] ActivationCommands = new EntityActivationCommand[]
        {
            new EntityActivationCommand("TellMe", "talk", true ),
            new EntityActivationCommand("FollowMe", "talk", true),
            new EntityActivationCommand("StayHere", "talk", true),
            new EntityActivationCommand("HangOut", "talk", true),
            new EntityActivationCommand("SetPatrol", "talk", true),
            new EntityActivationCommand("EndSetPatrol", "talk", true),
            new EntityActivationCommand("Patrol", "talk", true)
        };

        return ActivationCommands;
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos, EntityAlive _entityFocusing)
    {
        switch (_indexInBlockActivationCommands)
        {
            case 0: // Tell me about yourself
                GameManager.ShowTooltipWithAlert(_entityFocusing as EntityPlayerLocal, this.ToString() + "\n\n\n\n\n", "ui_denied");
                break;
            case 1: // Follow me
                this.Buffs.SetCustomVar("$Leader", _entityFocusing.entityId, true);
                this.Buffs.SetCustomVar("$CurrentOrder", (float)Orders.Follow, true);
                break;
            case 2: // Stay Here
                this.Buffs.SetCustomVar("$Leader", 0, true);
                this.Buffs.SetCustomVar("$CurrentOrder", (float)Orders.Stay, true);
                break;
            case 3: // Hang out / wander here
                this.Buffs.SetCustomVar("$Leader", 0, true);
                this.Buffs.SetCustomVar("$CurrentOrder", (float)Orders.Wander, true);
                break;
            case 4: // Set Patrol Point
                this.Buffs.SetCustomVar("$Leader", _entityFocusing.entityId, true);
                this.Buffs.SetCustomVar("$CurrentOrder", (float)Orders.SetPatrolPoint, true);
                break;
            case 5: // end patrol Point
                this.Buffs.SetCustomVar("$Leader", 0, true);
                this.Buffs.SetCustomVar("$CurrentOrder", (float)Orders.Patrol, true);
                break;

            
            case 6: // end patrol Point
                this.Buffs.SetCustomVar("$Leader", 0, true);
                break;
            default:
                break;
        }

        return true;
    }

    public override void PostInit()
    {
        base.PostInit();
        InvokeRepeating("DisplayStats", 0f, 60f);
    }


    public virtual void UpdatePatrolPoints( Vector3 position )
    {
        DisplayLog(" Checking Potential Patrol Point: " + position);
        Vector3i vector3i = new Vector3i(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y + 2.1f), Utils.Fastfloor(position.z));
        DisplayLog(" Vector3i Equievalent: " + vector3i.ToString());
        if (!this.PatrolCoordinates.Contains(vector3i.ToVector3()))
        {
            DisplayLog(" Vector added to list: " + vector3i.ToString());
            this.PatrolCoordinates.Add(vector3i.ToVector3());
        }
        else
        {
            DisplayLog(" Vector NOT added to list.");
        }

    }
    // Reads the buff and quest information
    public override void Read(byte _version, BinaryReader _br)
    {
        base.Read(_version, _br);
        this.strMyName = _br.ReadString();
        this.Buffs.Read(_br);
        this.QuestJournal = new QuestJournal();
        this.QuestJournal.Read(_br);
       // this.PatrolCoordinates.Clear();
        //foreach (String strPatrolPoint in _br.ReadString().Split(';'))
        //{
        //    DisplayLog(" Potential Patrol Point: " + strPatrolPoint);
        //    Vector3 temp = StringToVector3(strPatrolPoint);
        //    if ( temp != Vector3.zero )
        //        this.PatrolCoordinates.Add( temp );
        //}

    }

    public Vector3 StringToVector3(string sVector)
    {
        if (String.IsNullOrEmpty(sVector))
            return Vector3.zero;

        DisplayLog("String to Vector: " + sVector);
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    // Saves the buff and quest information
    public override void Write(BinaryWriter _bw)
    {
        base.Write(_bw);
        _bw.Write(this.strMyName);
        this.Buffs.Write(_bw, true);
        this.QuestJournal.Write(_bw);
        String strPatrolCoordinates ="";
        foreach (Vector3 temp in this.PatrolCoordinates)
        {
            DisplayLog(" Writing Patrol Coordinate: " + temp);
            strPatrolCoordinates = ";" + temp;
        }
       // _bw.Write(strPatrolCoordinates);
    }

    public void DisplayStats()
    {
        DisplayLog(ToString());
    }

    public override string ToString()
    {
        String FoodAmount = ((float)Mathf.RoundToInt(this.Stats.Stamina.ModifiedMax + this.Stats.Entity.Buffs.GetCustomVar("foodAmount"))).ToString() ;
        String WaterAmount = ((float)Mathf.RoundToInt(this.Stats.Water.Value + this.Stats.Entity.Buffs.GetCustomVar("waterAmount"))).ToString();
        String strSanitation = "Disabled.";
        if (this.Buffs.HasCustomVar("$solidWasteAmount"))
            strSanitation = this.Buffs.GetCustomVar("$solidWasteAmount").ToString();

        string strOutput = this.strMyName + " The " + this.entityName + " - ID: " + this.entityId + " Health: " + this.Stats.Health.Value;
        strOutput += " Stamina: " + this.Stats.Stamina.Value + " Thirst: " + this.Stats.Water.Value + " Food: " + FoodAmount + " Water: " + WaterAmount;
        strOutput += " Sanitation: " + strSanitation;

        if (this.Buffs.HasCustomVar("$CurrentOrder"))
            strOutput += " Current Order: " + (Orders)(int)this.Buffs.GetCustomVar("$CurrentOrder");

        strOutput += "\n Active Buffs: ";
        foreach (BuffValue buff in this.Buffs.ActiveBuffs)
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";

        strOutput += "\n Active Quests: ";
        foreach (Quest quest in this.QuestJournal.quests)
            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;

        strOutput += "\n Patrol Points: ";
        foreach (Vector3 vec in this.PatrolCoordinates)
            strOutput += "\n\t" + vec.ToString();
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
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, default(FastTags), 1f);
            else
                itemStack.count = forId.Stacknumber.Value;
            this.inventory.SetItem(i, itemStack);
        }
    }
   
    public override void OnUpdateLive()
    {
        // Non-player entities don't fire all the buffs or stats, so we'll manually fire the water tick,
        this.Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        this.Stats.UpdateStatsOverTime(0.5f);

        // Make the entity sensitive to the environment.
        this.Stats.UpdateWeatherStats(0.5f, this.world.worldTime, false);
        base.OnUpdateLive();
    }

   
  
}
