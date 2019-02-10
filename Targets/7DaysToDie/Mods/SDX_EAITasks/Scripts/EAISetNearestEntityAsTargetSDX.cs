using System;
using System.Collections.Generic;

    class EAISetNearestEntityAsTargetSDX : EAISetNearestEntityAsTarget
    {
    public override bool CanExecute()
    {
        bool result = base.CanExecute();
        if ( result )
        {
            // Don't target our master!
            if (this.theEntity.otherEntitySDX != null && base.targetEntity != null && base.targetEntity == this.theEntity.otherEntitySDX)
                return false;
        }
        return result;
    }
}

