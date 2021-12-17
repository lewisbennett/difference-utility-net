using MvvmCross.ViewModels;
using Sample.Assets;

namespace Sample.MvvmCross.Core;

public class PersonModel : MvxNotifyPropertyChanged
{
    private Person _person;

    public string Name => $"{_person.FirstName} {_person.LastName}";

    public int ID => _person.ID;

    public Person Person
    {
        get => _person;

        set
        {
            _person = value;

            RaisePropertyChanged(() => Name);
            RaisePropertyChanged(() => ID);
        }
    }
}