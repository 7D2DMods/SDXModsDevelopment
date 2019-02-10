using UnityEngine;
public class MinEventActionPumpQuestSDX : MinEventActionRemoveBuff
{
    // This loops through all the targets, refreshing the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="PumpQuestSDX, Mods" target="self"  />
    public override void Execute(MinEventParams _params)
    {
        for (int j = 0; j < this.targets.Count; j++)
        {
            EntityFarmingAnimal entity = this.targets[j] as EntityFarmingAnimal;
            if (entity != null)
            {
                for (int k = 0; k < entity.myQuestJournal.quests.Count; k++)
                {
                    for (int l = 0; l < entity.myQuestJournal.quests[k].Objectives.Count; l++)
                    {
                        entity.myQuestJournal.quests[k].Objectives[l].Refresh();
                    }
                }
            }
        }
    }
}
