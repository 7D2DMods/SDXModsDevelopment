using System.Collections.Generic;
using System.Xml;
using UnityEngine;
public class MinEventActionSpawnEntitySDX : MinEventActionRemoveBuff
{

    string strSpawnEntity = "";
    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="SpawnEntitySDX, Mods" target="self" entityname="mynewentity" />
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityAlive entity = this.targets[j] as EntityAlive;
            if (entity != null)
            {
                if (string.IsNullOrEmpty(this.strSpawnEntity))
                    continue;

                int EntityID = 0;

                // If the SpawnEntity key is "Same", then assume it's just a dupe of the target entity.
                if (this.strSpawnEntity == "Same")
                {
                    EntityID = entity.entityClass;
                }
                else
                {
                    foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
                    {
                        if (keyValuePair.Value.entityClassName == this.strSpawnEntity)
                        {
                            // we'll only need their entity ID to store, not their name.
                            EntityID = keyValuePair.Key;
                            break;
                        }
                    }
                }

                if (EntityID == 0)
                    break;

                Entity NewEntity = EntityFactory.CreateEntity(EntityID, entity.position, entity.rotation);

                NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
                GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
                if (NewEntity is EntityAliveSDX)
                {
                    EntityAliveSDX myBaby = NewEntity as EntityAliveSDX;
                    myBaby.otherEntitySDX = entity;
                }
                Debug.Log("An entity was created: " + NewEntity.name);
            }

        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "entityname")
                {
                    this.strSpawnEntity = _attribute.Value;
                    return true;
                }
            }
        }
        return flag;
    }
}
