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

    public override void OnAddedToWorld()
    {
   
    

    DisplayLog("EntityClass: " + this.entityClass);

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
                    
                    SpawnEntity(EntityClass.FromString(this.strLeaderEntity), true);
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
                            SpawnEntity(EntityClass.FromString(strEntity), false);
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

        this.MarkToUnload();
    }

    public void SpawnFromGroup( String strGroup, int Count )
    {
        int EntityID = -1;

        for (int x = 0; x < this.MaxSpawn; x++)
        {
            DisplayLog(" Spawning from : " + strGroup);
            EntityID = EntityGroups.GetRandomFromGroup(strGroup);
            SpawnEntity(EntityID, false);
        }

    }

    public void SpawnEntity( int EntityID, bool isLeader )
    {
        // Grab a random position.
        Vector3 transformPos;
        if (!this.world.GetRandomSpawnPositionMinMaxToPosition(this.world.GetPrimaryPlayer().position, 2, 6, 2, true, out transformPos, false))
        {
            DisplayLog(" No position available");
            return;
        }
        Entity NewEntity = EntityFactory.CreateEntity(EntityID, transformPos);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);

            if (isLeader)
            {
                DisplayLog(" Leader Entity ID: " + NewEntity.entityId);
                this.LeaderEntityID = NewEntity.entityId;
            }
            // Set the leaderID if its configured.
            else if (LeaderEntityID > 0 && NewEntity is EntityAliveSDX)
            {
                DisplayLog(" Setting Leader ID to: " + this.LeaderEntityID);
                (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("Leader", this.LeaderEntityID, true);
                (NewEntity as EntityAliveSDX).Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Follow, true);

            }
        }

    }
}

