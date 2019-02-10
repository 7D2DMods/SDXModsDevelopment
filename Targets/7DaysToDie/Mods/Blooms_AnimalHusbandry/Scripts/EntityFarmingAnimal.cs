/*
 * Class: EntityFarmingAnimal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive, and allows animal husbandry... breeding, etc
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityFarmingAnimal, Mods" />
 */
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

class EntityFarmingAnimal : EntityAlive
{

    private MemoryStream buffData = new MemoryStream(0);


    public String strFoodItem = "";
    public String strProductItem = "";
    public String strHarvestItems = "";
    public String strHomeBlock = "";
    public String strHomeBuff = "";
    public String strMaleEntity = "";
    public String strPregnancyBuff = "";
    public String strHarvestableBuff = "";
    public String strAdultBuff = "";

    public String strPregnancyQuest = "";
    public List<Quest> Quests = new List<Quest>();

    public QuestJournal myQuestJournal = new QuestJournal();

    // how far the animal will wander from its Home position.
    private int MaxDistanceFromHome = 15;
    private float MaxDistanceToSeePlayer = 20f;
    private float HarvestDelay = 10f;

    // Introduce some check delays to reduce the amount of calls per tick on them
    float CheckDelay = 5f;
    float LongerDelay = 3600f;
    float nextLongerCheck = 0;
    bool CheckEvent = false;

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
        if (entityClass.Properties.Values.ContainsKey("HomeBuff"))
            this.strHomeBuff = entityClass.Properties.Values["HomeBuff"];
        if (entityClass.Properties.Values.ContainsKey("MaleEntity"))
            this.strMaleEntity = entityClass.Properties.Values["MaleEntity"];
        if (entityClass.Properties.Values.ContainsKey("PregnancyBuff"))
            this.strPregnancyBuff = entityClass.Properties.Values["PregnancyBuff"];
        if (entityClass.Properties.Values.ContainsKey("HarvestableBuff"))
            this.strHarvestableBuff = entityClass.Properties.Values["HarvestableBuff"];
        if (entityClass.Properties.Values.ContainsKey("AdultBuff"))
            this.strAdultBuff = entityClass.Properties.Values["AdultBuff"];

        if (entityClass.Properties.Values.ContainsKey("StartingGrowthBuff"))
            this.Buffs.AddBuff(entityClass.Properties.Values["StartingGrowthBuff"], -1, true);

        if (entityClass.Properties.Values.ContainsKey("PregnancyQuest"))
            this.strPregnancyQuest = entityClass.Properties.Values["PregnancyQuest"];


    }

    public override void Read(byte _version, BinaryReader _br)
    {
        Debug.Log("Read");
        base.Read(_version, _br);
      //  int num2 = _br.ReadInt32();
        this.Buffs.Read(_br);
        //this.buffData = ((num2 <= 0) ? new MemoryStream() : new MemoryStream(_br.ReadBytes(num2)));
        //if (this.buffData.Length > 0L)
        //{
        //    Debug.Log("No buffs");
        //    if (this.Buffs == null)
        //    {
        //        this.Buffs = new EntityBuffs(this);
        //    }
        //    using (PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(false))
        //    {
        //        pooledBinaryReader2.SetBaseStream(this.buffData);
        //        this.Buffs.Read(pooledBinaryReader2);
        //    }
        //}
        //else
        //{
        //    Debug.Log("No Buffs Detected.");
        //}

        this.myQuestJournal = new QuestJournal();
        this.myQuestJournal.Read(_br);

    }

    public override void Write(BinaryWriter _bw)
    {
        Debug.Log("Write");
        base.Write(_bw);
        this.Buffs.Write(_bw, true);
       // _bw.Write(this.Buffs);
      
      //  _bw.Write((int)this.buffData.Length);
      //  Utils.StreamCopy(this.buffData, _bw.BaseStream, null, true);
        this.myQuestJournal.Write(_bw);

    }
    public override void PostInit()
    {
        base.PostInit();
   
        String strMessage = "Creating " + this.entityName;
        strMessage += " Food Item: " + this.strFoodItem;
        strMessage += " Product Item: " + this.strProductItem;
        strMessage += " Harvest Items: " + this.strHarvestItems;
        strMessage += " Home Block: " + this.strHomeBlock;
        //strMessage += " Home Buff: " + this.strHomeBuff;
        DisplayLog(strMessage);

        // trigger 60 seconds after the start, then every 60 seconds afterwards.
        InvokeRepeating("CheckAnimalEvent", 60f, 60f);

        DisplayLog(ToString());
        GiveQuest(this.strPregnancyQuest);

    }
    public override string ToString()
    {
        string strOutput = this.entityName + " - ID: " + this.entityId + " Health: " + this.Health + " Hunger: " + this.Stats.Stamina.Value + " Thirst: " + this.Stats.Water.Value;
        strOutput += "\n Active Buffs: ";
        foreach (BuffValue buff in this.Buffs.ActiveBuffs)
        {
            strOutput += "\n\t" + buff.BuffName + " ( Seconds: " + buff.DurationInSeconds + " Ticks: " + buff.DurationInTicks + " )";
        }

        strOutput += "\n Active Quests: ";
        foreach (Quest quest in this.myQuestJournal.quests)
        {
            //foreach (ObjectiveBuff buff in quest.Objectives)
            //    buff.Refresh();

            strOutput += "\n\t" + quest.ID + " Current State: " + quest.CurrentState + " Current Phase: " + quest.CurrentPhase;
        }

        return strOutput;
    }


    public void GiveQuest(String strQuest)
    {
        foreach (Quest quest in this.myQuestJournal.quests)
        {
            if (quest.ID == strQuest)
                return;
        }
        Quest NewQuest = QuestClass.CreateQuest(strQuest);
        if (NewQuest == null)
            return;

        // If there's no shared owner, it tries to read the PlayerLocal's entity ID. This entity doesn't have that.
        NewQuest.SharedOwnerID = this.entityId;
        NewQuest.QuestGiverID = -1;
        this.myQuestJournal.AddQuest(NewQuest);
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
            case 6: // morning harvest time

                // Allowed To Harvest: This can be a condition that the harvest is only allowed after the entity has had a baby, for example.
                if (HasBuff(this.strAdultBuff))
                {
                    // If it's 6am, give them their harvest item into their inventory. But only allow one.
                    ItemValue newItem = ItemClass.GetItem(this.strProductItem, false);
                    if (this.inventory.GetItemCount(newItem, false, -1, -1) == 0)
                        this.inventory.AddItem(new ItemStack(newItem, 1));
                }
                break;
            case 21:
                // Nearly night time event
                FindPotentialFather();
                break;
            case 22:
                // night time
                break;
        }

        // Awwwwwwww we had a baby
        if (HasBuff("buffNewMother"))
        {
            //SpawnNewBaby
            this.Buffs.RemoveBuff("buffNewMother", true);
            SpawnBaby();
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
    public bool HasBuff(String strBuff)
    {
        return this.Buffs.ActiveBuffs.Contains(Buffs.GetBuff(strBuff));
    }
    public void CheckForPlayersWithFood()
    {
        // If the entity can see a player, and the player is holding its food item, start moving towards the player.
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * MaxDistanceToSeePlayer));
        if (entitiesInBounds.Count > 0)
        {
            for (int i = 0; i < entitiesInBounds.Count; i++)
            {
                if (entitiesInBounds[i] is EntityPlayer)
                {
                    EntityPlayer entityPlayer = entitiesInBounds[i] as EntityPlayer;

                    if (entityPlayer.inventory.holdingItem.Name == strFoodItem)
                    {
                        DisplayLog("I am following " + entityPlayer.EntityName + " because they have yummy " + strFoodItem);
                        //SetInvestigatePosition(entityPlayer.GetBlockPosition().ToVector3(), 5);
                        SetAttackTarget(entityPlayer, 10);
                    }

                }
            }

        }

    }
    public void SpawnBaby()
    {
        Vector3 vector = this.position;
        Entity entity2 = EntityFactory.CreateEntity(EntityClass.FromString(this.entityName), vector, new Vector3(0f, UnityEngine.Random.value * 360f, 0f));
        entity2.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        entity2.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
        this.world.SpawnEntityInWorld(entity2);
        // Play a sound on birth?
        //    _world.GetGameManager().PlaySoundAtPositionServer(this.position.ToVector3(), entitySpawnerClass.startSound, AudioRolloffMode.Custom, 300);
    }
    public override void OnUpdateLive()
    {

        // Update it's entity stats.
        this.Stats.UpdateStatsOverTime(0.05f);
        base.OnUpdateLive();



        // Quicker update to check if there's any entities around
        if (CheckDelay < Time.time)
        {

          //  CheckForPlayersWithFood();

            // Check if there's an animal event that needs to happen.
            //   CheckAnimalEvent();
        }


    }

    private void FindPotentialFather()
    {
        if (this.IsMale || this.strMaleEntity == "")
            return;

        // Already pregnant? Brand New Mother? Not An Adult?
        if (HasBuff(strPregnancyBuff) || HasBuff("buffNewMother") || !HasBuff(this.strAdultBuff))
            return;

        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 4)); // 4 blocks away scan for daddy
        if (entitiesInBounds.Count > 0)
        {
            for (int i = 0; i < entitiesInBounds.Count; i++)
            {
                if (entitiesInBounds[i] is EntityFarmingAnimal)
                {
                    // If the Farm Animal is male and the same type, then allow them to breed
                    EntityFarmingAnimal entityAnimal = entitiesInBounds[i] as EntityFarmingAnimal;
                    if (entityAnimal.IsMale && entityAnimal.entityName == this.strMaleEntity)
                        this.Buffs.AddBuff(strPregnancyBuff, -1, true);
                }
            }

        }

    }
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
        //if (this.IsDead())
        //    return new EntityActivationCommand[0];
        DisplayLog("GetActivationCommands");
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
            GameManager.ShowTooltipWithAlert(_entityFocusing as EntityPlayerLocal, strDisplay, "ui_denied");
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
