using System;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using Sample.MvvmCross.Core;

namespace Sample.MvvmCross.Droid;

public class RecyclerViewTemplateSelector : MvxTemplateSelector<object>
{
    public override int GetItemLayoutId(int fromViewType)
    {
        return fromViewType;
    }

    protected override int SelectItemViewType(object forItemObject)
    {
        return forItemObject switch
        {
            PersonModel => Resource.Layout.item_person,
            _ => throw new NotImplementedException($"Layout not implemented for {forItemObject?.GetType()}")
        };
    }
}