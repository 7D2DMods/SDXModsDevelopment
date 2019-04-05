using UnityEngine;
class XUiC_JoinInformationPopupSDX : XUiC_HireInformationPopupSDX
{
    public override void OnOpen()
    {
        LocalPlayerUI uiforPlayer = base.xui.playerUI;

        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent == null)
            respondent = uiforPlayer.xui.Dialog.otherEntitySDX;

        if (respondent != null)
        {
            EntityAliveSDX myEntity = uiforPlayer.entityPlayer.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                this.hireInformationLabel.Text = Localization.Get("HireOffer_" + myEntity.EntityName, "");
                if ( this.hireInformationLabel.Text == "Hire_Offer_" + myEntity.EntityName )
                {
                    this.hireInformationLabel.Text = "I would like to join you. Will you accept me?";
                }
            }
        }

        base.OnOpen();
       
    }

    private void BtnConfirmHireInformation_OnPressed(XUiController _sender, OnPressEventArgs _onPressEventArgs)
    {
        LocalPlayerUI uiforPlayer = base.xui.playerUI;

        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent == null)
            respondent = uiforPlayer.xui.Dialog.otherEntitySDX;

        if (respondent != null)
        {
            EntityAliveSDX myEntity = uiforPlayer.entityPlayer.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if (myEntity != null)
            {
                myEntity.SetOwner(uiforPlayer.entityPlayer as EntityPlayerLocal);
            }
        }

        base.xui.playerUI.windowManager.Close(this.windowGroup.ID);
    }

  
   
}

