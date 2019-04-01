using UnityEngine;
class RewardNPCSDX : RewardItem
{
    //		<reward type="NPCSDX, Mods" id="entityGroup"  />  // Spawns in an entity from the group to be your NPC
    //		<reward type="NPCSDX, Mods"  />  // Hires the current NPC
    public override void GiveReward(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(base.ID))
        {
            EntityAliveSDX questNPC = GameManager.Instance.World.Entities.dict[base.OwnerQuest.QuestGiverID] as EntityAliveSDX;
            if (questNPC)
                questNPC.SetOwner(player as EntityPlayerLocal);
        }
        else   // Try to spawn in a new NPC from the NPC Group
            SpawnFromGroup(base.ID, player);
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

        Entity NewEntity = EntityFactory.CreateEntity(EntityID, player.position, player.rotation);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
            Debug.Log("An entity was created: " + NewEntity.ToString());
            if (NewEntity is EntityAliveSDX)
            {
                (NewEntity as EntityAliveSDX).SetOwner(player as EntityPlayerLocal);
            }
                
        }
        else
        {
            Debug.Log(" Could not Spawn NPC for: " + player.EntityName + " : " + player.entityId);
        }
    }
}

