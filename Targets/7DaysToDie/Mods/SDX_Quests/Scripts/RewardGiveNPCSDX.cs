using UnityEngine;
class RewardGiveNPCSDX : RewardExp
{
    //		<reward type="GiveNPCSDX, Mods" id="entityGroup"  />  // Spawns in an entity from the group to be your NPC
    //		<reward type="GiveNPCSDX, Mods"  />  // Hires the current NPC
    public override void GiveReward(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(base.ID))
        {
            Debug.Log(" Searching for NPC Entity ID: " + base.OwnerQuest.QuestGiverID);
            EntityAliveSDX questNPC = GameManager.Instance.World.Entities.dict[base.OwnerQuest.QuestGiverID] as EntityAliveSDX;
            if (questNPC)
            {
                Debug.Log(" Assigning " + questNPC.EntityName + " to " + player.entityId);
                questNPC.SetOwner(player as EntityPlayerLocal);
            }
            else
            {
                Debug.Log(" NPC not Found.");
            }
        }
        else   // Try to spawn in a new NPC from the NPC Group
        {
            Debug.Log(" Spawning From Entity Group: " + base.ID);
            SpawnFromGroup(base.ID, player);
        }
    }

    public void SpawnFromGroup( string strEntityGroup, EntityPlayer player )
    {
        int EntityID = 0;

        // If the group is set, then use it.
        if (string.IsNullOrEmpty(strEntityGroup))
            return;

        EntityID = EntityGroups.GetRandomFromGroup(strEntityGroup);
        if (EntityID == -1)
            return; // failed

        Debug.Log("Spawning From Group..." + strEntityGroup + " - " + EntityID);
        Entity NewEntity = EntityFactory.CreateEntity(EntityID, player.position, player.rotation);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
            Debug.Log("An entity was created: " + NewEntity.ToString());
            if (NewEntity is EntityAliveSDX)
            {
                Debug.Log(" Assigning new NPC to Player: " + (NewEntity as EntityAliveSDX).EntityName + " Player: " + player.EntityName);
                (NewEntity as EntityAliveSDX).SetOwner(player as EntityPlayerLocal);
            }
                
        }
        else
        {
            Debug.Log(" Could not Spawn NPC for: " + player.EntityName + " : " + player.entityId);
        }
    }
}

