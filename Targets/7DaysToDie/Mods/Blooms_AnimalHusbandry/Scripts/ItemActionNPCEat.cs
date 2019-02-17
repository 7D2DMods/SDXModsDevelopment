using System;
using UnityEngine;
class ItemActionExchangeItemSDX: ItemActionExchangeItem
{

    private bool isFocusingBlock(WorldRayHitInfo _hitInfo)
    {
        for (int i = 0; i < this.focusedBlocks.Count; i++)
        {
            BlockValue other = this.focusedBlocks[i];
            if (_hitInfo.hit.blockValue.Equals(other))
            {
                return true;
            }
        }
        return false;
    }

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        Debug.Log("ExecuteAction ExchangeItemSDX");
        if (_bReleased && Voxel.voxelRayHitInfo.bHitValid && this.isFocusingBlock(Voxel.voxelRayHitInfo))
        {

            Debug.Log("Checking actions");
            if (_actionData.indexInEntityOfAction == 0)
            {
                Debug.Log("Primary action");
                _actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfPrimaryActionStart);
            }
            else
            {
                Debug.Log("Secondary Action");
                _actionData.invData.holdingEntity.FireEvent(MinEventTypes.onSelfSecondaryActionStart);
            }

        }

        base.ExecuteAction(_actionData, _bReleased);



    }


}

