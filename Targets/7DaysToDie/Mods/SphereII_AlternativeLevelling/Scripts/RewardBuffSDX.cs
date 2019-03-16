using System;

//		<reward type="BuffSDX, Mods"  value="buffName" />

class RewardBuffSDX : RewardQuest
{

    public override BaseReward Clone()
    {
        RewardBuffSDX rewardBuff = new RewardBuffSDX();
        base.CopyValues(rewardBuff);
    
        return rewardBuff;
    }


    public override void GiveReward(EntityPlayer player)
    {
        player.Buffs.AddBuff(base.Value, -1, true);
    }

}

