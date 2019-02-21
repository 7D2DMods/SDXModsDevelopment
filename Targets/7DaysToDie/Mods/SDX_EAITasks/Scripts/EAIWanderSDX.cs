using System;
    class EAIWanderSDX : EAIWander
    {

    public override bool CanExecute()
    {
        return base.CanExecute();
    }

    public override bool Continue()
    {

        if (this.theEntity.moveHelper.BlockedTime <= 1f)
            return false;

        return base.Continue();
    }
}

