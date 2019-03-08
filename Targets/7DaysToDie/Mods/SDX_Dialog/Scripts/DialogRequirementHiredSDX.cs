using UnityEngine;
public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override BaseDialogRequirement.RequirementTypes RequirementType
    {
        get
        {
            return BaseDialogRequirement.RequirementTypes.Admin;
        }
    }

    public override void SetupRequirement()
    {
        string description = Localization.Get("RequirementAdmin_keyword", Localization.QuestPrefix);
        base.Description = description;
    }

    public override bool CheckRequirement(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        
        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent == null)
            respondent = uiforPlayer.xui.Dialog.otherEntitySDX;

        if (respondent != null)
        {
            EntityAliveSDX myEntity = player.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                if ( base.Value.EqualsCaseInsensitive("not"))
                    return !myEntity.isTame(player);
                else
                    return myEntity.isTame(player);
            }
         }
        return false;
    }



}


