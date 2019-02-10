using System;
using UnityEngine;
class ObjectiveBuffSDX : BaseObjective
{
    String strBuff = "";


    public override BaseObjective Clone()
    {
        ObjectiveBuffSDX objectiveBuff = new ObjectiveBuffSDX();
        this.CopyValues(objectiveBuff);
        return objectiveBuff;
    }
    protected void CopyValues(ObjectiveBuffSDX objective)
    {
        objective.ID = this.ID;
        objective.Value = this.Value;
        objective.Optional = this.Optional;
        objective.currentValue = this.currentValue;
        objective.Phase = this.Phase;
        objective.strBuff = this.strBuff;
    }

    //public override void Update(float deltaTime)
    //{
    //    this.Refresh();
    //    base.Update(deltaTime);
    //}

    public override void AddHooks()
    {
        
    }

    public override void RemoveHooks()
    {
        
    }
    public override void Refresh()
    {
        if (string.IsNullOrEmpty(this.strBuff))
            return;

        if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
        {
            EntityAlive myEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAlive;
            if (myEntity != null)
            {
                base.Complete = myEntity.Buffs.HasBuff(this.strBuff);
                if (base.Complete)
                {
                    base.ObjectiveState = ObjectiveStates.Complete;

                    base.OwnerQuest.CheckForCompletion(QuestClass.CompletionTypes.AutoComplete, null);
                }
            }
        }

    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);

        if (properties.Values.ContainsKey("buff"))
            this.strBuff = properties.Values["buff"];


    }

    protected override bool useUpdateLoop
    {
        get
        {
            return true;
        }
    }
}
