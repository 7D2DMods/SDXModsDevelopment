    using UnityEngine;
using System.Collections.Generic;
class RewardReassignNPCSDX : BaseReward
{
    // If the QuestNPC has other NPCs that have assigned it as their leader, this class will transfer the leadership flag to the player.
    //		<reward type="ReassignNPCSDX, Mods"  /> 
    public override void GiveReward(EntityPlayer player)
    {

        EntityAliveSDX questNPC = GameManager.Instance.World.Entities.dict[base.OwnerQuest.QuestGiverID] as EntityAliveSDX;
        if (questNPC)
             CheckSurroundingEntities(questNPC, player );
    }

    public void CheckSurroundingEntities(EntityAliveSDX questNPC, EntityPlayer player )
    {
        List<Entity> NearbyEntities = new List<Entity>();
        Bounds bb = new Bounds(questNPC.position, new Vector3(questNPC.GetSeeDistance() , 20f, questNPC.GetSeeDistance()));
        questNPC.world.GetEntitiesInBounds(typeof(EntityAliveSDX), bb, NearbyEntities);
        for (int i = NearbyEntities.Count - 1; i >= 0; i--)
        {
            EntityAliveSDX x = (EntityAliveSDX)NearbyEntities[i];
            if (x != questNPC && x.IsAlive())
            {
                if (x.Buffs.HasCustomVar("Leader") && x.Buffs.GetCustomVar("Leader") == (float)questNPC.entityId)
                {
                    x.SetOwner(player as EntityPlayerLocal);
                }
            }
        }
    }
}

