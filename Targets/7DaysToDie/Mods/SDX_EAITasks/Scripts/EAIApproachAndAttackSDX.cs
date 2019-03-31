using System;
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;
    public override void Start()
    {
        base.Start();
        isTargetToEat = false;
    }

}

