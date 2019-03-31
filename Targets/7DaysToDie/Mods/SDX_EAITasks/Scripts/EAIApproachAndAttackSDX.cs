using System;

//<property name="AITask-5" value="ApproachAndAttackTargetSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;
    public override void Start()
    {
        base.Start();
        isTargetToEat = false;
    }

}

