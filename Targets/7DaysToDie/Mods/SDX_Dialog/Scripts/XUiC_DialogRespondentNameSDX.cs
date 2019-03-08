public class XUiC_DialogRespondentNameSDX : XUiC_DialogRespondentName
{
    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        string fieldName = binding.FieldName;
        if (fieldName != null)
        {
            if (fieldName == "respondentname")
            {
                if ( base.xui.Dialog.otherEntitySDX != null )
                {
                        value = base.xui.Dialog.otherEntitySDX.EntityName;
                        return true;
                }
                value = ((!(base.xui.Dialog.Respondent != null)) ? string.Empty : Localization.Get(base.xui.Dialog.Respondent.EntityName, string.Empty));
               return true;
            }

        }
        return false;
    }
}