using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DifferenceUtility.Net;
using DifferenceUtility.Net.Helper;
using Sample.NetConsole.PathDeconstruction;

// Replace these strings to test difference calculation. The strings must only contain unique characters (i.e. no duplicates).
const string source = "badfhlocz";
const string destination = "azhbdfc";

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

var diffPath = diffResult.GetPath();

var currentX = -1;
var currentY = -1;

// The below contains logic for when move detection is enabled.
// If move detection is disabled, no payload will have the Move flag, and therefore such checks aren't necessary.
foreach (var payload in diffPath)
{
    if ((payload & DiffOperation.Insert) != 0)
    {
        currentY++;

        // X coordinate is encoded if the payload has the Move flag.
        if ((payload & DiffOperation.Move) != 0)
            Console.WriteLine($"+ {destination[currentY]}");

        else
            Console.WriteLine($"+ {destination[payload >> DiffOperation.Offset]}");

        continue;
    }

    if ((payload & DiffOperation.Remove) != 0)
    {
        currentX++;

        // Y coordinate is encoded if the payload has the Move flag.
        if ((payload & DiffOperation.Move) != 0)
            Console.WriteLine($"- {source[currentX]}");

        else
            Console.WriteLine($"- {source[payload >> DiffOperation.Offset]}");

        continue;
    }

    // If neither insert nor remove, increment both coordinates.
    currentX++;
    currentY++;

    // Both if statements do the same job.
    if ((payload & DiffOperation.Update) != 0)
        Console.WriteLine($"~ {source[currentX]}");

    else
        Console.WriteLine($"  {source[currentX]}");

    // if ((payload & DiffOperation.Update) != 0)
    //     Console.WriteLine($"~ {destination[currentY]}");
    //
    // else
    //     Console.WriteLine($"  {destination[currentY]}");
}

// Apply the updates.
diffResult.DispatchUpdatesTo(data);

foreach (var item in data)
    Console.Write(item);