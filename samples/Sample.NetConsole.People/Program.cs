using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using DifferenceUtility.Net;
using Sample.Assets;
using Sample.NetConsole.People;

var data = new ObservableCollection<Person>();

data.CollectionChanged += (s, e) =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:

            Console.WriteLine($"Item added at index: {e.NewStartingIndex}");

            break;

        case NotifyCollectionChangedAction.Remove:

            Console.WriteLine($"Item removed at index: {e.OldStartingIndex}");

            break;

        case NotifyCollectionChangedAction.Move:

            Console.WriteLine($"Item moved from index: {e.OldStartingIndex} to {e.NewStartingIndex}");

            break;

        default:

            throw new NotImplementedException();
    }
};

// Request collection of people.
var people = await API.GetPeopleAsync();

var diffCallback = new PersonDiffCallback();

// Calculate the difference between the old datasource and the new datasource.
// We already know that the items source is empty, but DiffUtil can add items when there is no pre-existing data.
// This might, however, be slower than adding the data normally, but can save on code size and it may come in handy
// in rare scenarios where the state of the items source is unknown.
var result = DiffUtil.CalculateDiff(data, people, diffCallback);

// Apply the updates.
result.DispatchUpdatesTo(data);

Console.WriteLine();

foreach (var item in data)
    Console.WriteLine(item.ToString());

Console.WriteLine();

// Some time has passed - the remote data has changed.
await Task.Delay(TimeSpan.FromSeconds(2));

// Get the new data.
people = await API.GetPeopleAsync();

result = DiffUtil.CalculateDiff(data, people, diffCallback);

result.DispatchUpdatesTo(data);

Console.WriteLine();

foreach (var item in data)
    Console.WriteLine(item.ToString());

Console.ReadLine();