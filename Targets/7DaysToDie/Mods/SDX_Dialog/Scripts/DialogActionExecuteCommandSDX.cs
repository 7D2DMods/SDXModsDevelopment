using UnityEngine;
public class DialogActionExecuteCommandSDX : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType
    {
        get
        {
            return BaseDialogAction.ActionTypes.AddBuff;
        }
    }

    public override void PerformAction(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        // The respondent is an EntityNPC, and we don't have that. Check for the patch scripted otherEntitySDX.
        Entity respondent = uiforPlayer.xui.Dialog.Respondent;
        if (respondent == null)
            respondent = uiforPlayer.xui.Dialog.otherEntitySDX;

        if (respondent != null)
        {
            EntityAliveSDX myEntity = player.world.GetEntity(respondent.entityId) as EntityAliveSDX;
            if ( myEntity != null )
            {
                string strCommand = base.ID;
                Debug.Log(GetType().ToString() + " : Command: " + strCommand);
                switch( strCommand)
                {
                    case "ShowMe":
                        GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, myEntity.ToString() + "\n\n\n\n\n", "ui_denied");
                        break;
                    case "FollowMe":
                        myEntity.Buffs.SetCustomVar("Leader", player.entityId, true);
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Follow, true);
                        break;
                    case "StayHere":
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Stay, true);
                        break;
                    case "Wander":
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Wander, true);
                        break;
                    case "SetPatrol":
                        myEntity.Buffs.SetCustomVar("Leader", player.entityId, true);
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.SetPatrolPoint, true);
                        myEntity.PatrolCoordinates.Clear(); // Clear the existing point.
                        break;
                    case "Patrol":
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Patrol, true);
                        break;
                    case "Hire":
                        bool result = myEntity.Hire(player as EntityPlayerLocal);
                        if( result )
                        {
                            myEntity.Buffs.SetCustomVar("Leader", player.entityId, true);
                            myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Follow, true);
                        }
                        break;
                    case "OpenInventory":
                        GameManager.Instance.TELockServer(0, myEntity.GetBlockPosition(), myEntity.entityId, player.entityId);
                        break;
                    case "Loot":
                        myEntity.Buffs.SetCustomVar("CurrentOrder", (float)EntityAliveSDX.Orders.Loot, true);
                        break;

                }
            }
        }
    }

    private string name = string.Empty;
}
