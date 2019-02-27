using System.Collections.Generic;
using System.Xml;
using UnityEngine;
public class MinEventActionSpawnBabySDX : MinEventActionRemoveBuff
{
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityAliveSDX entity = this.targets[j] as EntityAliveSDX;
            if (entity )
            {
                int  EntityID = entity.entityClass;

                EntityAliveSDX NewEntity = EntityFactory.CreateEntity(EntityID, entity.position, entity.rotation) as EntityAliveSDX;
                if (NewEntity)
                {
                   
                    NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                    GameManager.Instance.World.SpawnEntityInWorld(NewEntity);

                    Debug.Log("An entity was created: " + NewEntity.ToString());

                    Debug.Log("Setting Mother ID to Baby: " + entity.entityId + " to " + NewEntity.entityId);
                    NewEntity.Buffs.SetCustomVar("$Mother", entity.entityId, true);

                }
                else
                {
                    Debug.Log(" Could not Spawn baby for: " + entity.EntityName + " : " + entity.entityId);
                }
            }

        }
    }
   
}
