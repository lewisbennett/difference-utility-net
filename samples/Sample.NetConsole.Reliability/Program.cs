using System.Collections.ObjectModel;
using System.Text;
using DifferenceUtility.Net;
using Sample.NetConsole.Reliability;

// Fill with strings that contain no duplicate characters.
var strings = new[]
{
    "abc",
    "wxyz",
    "badfhlocz",
    "azhbdfc",
    "nwpalgfbds",
    "mlfsgbadnhzx",
    "wa4ytg3297",
    "zopa93wgfh84",
    "laogh4ns208",
    "p1203na9sef8",
    "p1203na9sef8w4ytg7",
    "laogh4ns208zp93wf",
    "wa4ynpltg3297fbds",
    "zopadnhx93wgf84mlsb",
    "abdflns208zp93wchog",
    "abcdefghijklmnopqrstuvwxyz",
    "zyxwvutsrqponmlkjihgfedcba",
    "1234567890",
    "0987654321",
    "57d2ef09b4a8c3",
    "e198075db4a2fc36",
    "9c0d568bea47231f",
    "8c0a594326b7d1",
    "e873fcd624a5019b",
    "f9a734d8e2c1b56",
    "d48c0ab9312657f",
    "4e1b3982507adc6f"
};

// Sanity check - make sure there are only unique entries with distinct values.
var finalStrings = new List<string>(strings
    .Distinct()
    .Select(f => f
        .Distinct()
        .Aggregate(string.Empty, (current, @char) => current + @char)));

const int stringLengthMin = 1;
const int stringLengthMax = 50;

var random = new Random();

// Simple loop to generate some random sequences that we can use for additional testing.
for (var i = 0; i < 10; i++)
{
    var stringBuilder = new HashSet<char>();

    var index = 0;
    var stringLength = random.Next(stringLengthMin, stringLengthMax);
    
    while (index < stringLength)
    {
        var @char = (char)random.Next(0, 100);

        if (!stringBuilder.TryGetValue(@char, out _))
        {
            stringBuilder.Add(@char);
            index++;
        }
    }
    
    finalStrings.Add(string.Join(string.Empty, stringBuilder));
}

// Make sure we only have unique strings before beginning the test.
finalStrings = finalStrings
    .Distinct()
    .ToList();

var data = new ObservableCollection<char>();

var successes = new List<(string From, string To, string Result)>();
var failures = new List<(string From, string To, string Result)>();

foreach (var @string in finalStrings)
{
    foreach (var testString in strings)
    {
        if (@string == testString)
            continue;
        
        data.Clear();
        
        // Add the starting data.
        foreach (var @char in @string)
            data.Add(@char);
        
        var characterDiffCallback = new CharacterDiffCallback();

        // Calculate the difference between the old datasource and the new datasource.
        var diffResult = DiffUtil.CalculateDiff(@string, testString, characterDiffCallback);

        // Apply the updates.
        diffResult.DispatchUpdatesTo(data);

        var result = data.Aggregate(string.Empty, (current, @char) => current + @char);

        (result.Equals(testString) ? successes : failures).Add((@string, testString, result));
    }
}

Console.WriteLine("Done!");
Console.WriteLine($"Successes: {successes.Count}");
Console.WriteLine($"Failures: {failures.Count}");
Console.WriteLine();

foreach (var (from, to, result) in failures)
    Console.WriteLine($"Failure from: {from} to: {to} result: {result}");