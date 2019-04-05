using System;
using System.Collections.Generic;
using UnityEngine;
class EntityAliveEventSpawnerSDX : EntityAlive
{
    public String strEntityGroup = "";
    public int MaxSpawn = 1;

    String strLeaderEntity = "";
    int LeaderEntityID = -1;

    private bool blDisplayLog = true;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + strMessage);
    }

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        // base.Awake();
        this.entityClass = _entityClass;
    DisplayLog("EntityClass: " + _entityClass);

        EntityClass entityClass = EntityClass.list[this.entityClass];
        if (entityClass.Properties.Classes.ContainsKey("SpawnSettings"))
        {
            DisplayLog(" Found Spawn Settings.. reading...");
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["SpawnSettings"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                if (keyValuePair.Key == "Leader")
                {
                    DisplayLog(" Found a Leader");
                    this.strLeaderEntity = dynamicProperties3.Values[keyValuePair.Key];
                    this.LeaderEntityID = EntityClass.FromString(this.strLeaderEntity);
                    SpawnEntity(this.LeaderEntityID);
                    continue;
                }
                else if ( keyValuePair.Key == "Followers")
                {
                    DisplayLog("Found Followers");
                    // if it contains commas, it's individual entities.
                    String strValue = dynamicProperties3.Values[keyValuePair.Key];
                    if ( strValue.Contains(","))
                    {
                        foreach( String strEntity in strValue.Split(','))
                            SpawnEntity(EntityClass.FromString(strEntity));
                    }
                    else  //  Spawn from Entity Group
                    {
                        String strCount = "1";
                        dynamicProperties3.Params1.TryGetValue(keyValuePair.Key, out strCount);
                        SpawnFromGroup(strValue, int.Parse( strCount));
                    }
                }
                else if ( keyValuePair.Key.StartsWith("Follower-"))
                {
                    DisplayLog(" Found A Follower");
                    String strValue = dynamicProperties3.Values[keyValuePair.Key];
                    int minCount = 1;
                    int maxCount = 1;
                    string strRange = "";
                    dynamicProperties3.Params1.TryGetValue(keyValuePair.Key, out strRange);
                    StringParsers.ParseMinMaxCount(strRange, out minCount, out maxCount);
                    float Count = UnityEngine.Random.Range((float)minCount, (float)maxCount);
                    
                    SpawnFromGroup(strValue, int.Parse(Count.ToString() ));
                }
                else
                {
                    DisplayLog("Found nothing?");
                }

            }
        }
        else
        {
            DisplayLog(" No Spawn settings found.");
        }
    }

    public void SpawnFromGroup( String strGroup, int Count )
    {
        int EntityID = -1;

        for (int x = 0; x < this.MaxSpawn; x++)
        {
            DisplayLog(" Spawning from : " + strGroup);
            EntityID = EntityGroups.GetRandomFromGroup(strGroup);
            SpawnEntity(EntityID);
        }

    }

    public void SpawnEntity( int EntityID )
    {
        // Grab a random position.
        Vector3 transformPos;
        if (!this.world.GetRandomSpawnPositionMinMaxToPosition(this.position, 2, 6, 2, true, out transformPos, false))
            return;

        DisplayLog(" Spawn Entity: " + EntityID);
        Entity NewEntity = EntityFactory.CreateEntity(EntityID, transformPos);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
            // Set the leaderID if its configured.
            if (LeaderEntityID > 0 && NewEntity is EntityAliveSDX)
                (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("Leader", this.LeaderEntityID, true);
        }

    }
}

