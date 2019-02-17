using System;
using UnityEngine;
class ItemActionExchangeItemSDX: ItemActionExchangeItem
{

    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        Debug.Log("ExecuteAction ExchangeItemSDX");
        Debug.Log(" Released?: " + _bReleased);
        Debug.Log(" Last Use Count: " + _actionData.lastUseTime);
        if ( _bReleased && _actionData.lastUseTime > 0f )
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

