class EAIAnimalBloodMoonSDX : EAIBase
{

    public override bool CanExecute()
    {
        bool result = false;

        int day = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime());
        if (day % 7 == 0) // Blood Moon Day Events
        {
            result = true;
        }

        return result;
    }

    public override void Update()
    {
        base.Update();
    }
}