using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DifferenceUtility.Net;
using Sample.NetConsole.Characters;

// Replace these strings to test difference calculation. The strings must only contain unique characters (i.e. no duplicates).
const string source = "zopa93wgfh84";
const string destination = "abcdefghijklmnopqrstuvwxyz";

// Create our items source with initial data.
var data = new ObservableCollection<char>(source);

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

var characterDiffCallback = new CharacterDiffCallback();

// Calculate the difference between the old datasource and the new datasource.
var diffResult = DiffUtil.CalculateDiff(source, destination, characterDiffCallback);

// Apply the updates.
diffResult.DispatchUpdatesTo(data);

foreach (var item in data)
    Console.Write(item);
