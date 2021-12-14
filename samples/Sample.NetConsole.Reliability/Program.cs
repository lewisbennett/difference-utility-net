﻿using System.Collections.ObjectModel;
using DifferenceUtility.Net;
using Sample.NetConsole.Reliability;

// Fill with strings that contain no duplicate characters.
var strings = new[]
{
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
    "zopadnhx93wgf84mlsb"
};

var data = new ObservableCollection<char>();

var successes = new List<(string From, string To, string Result)>();
var failures = new List<(string From, string To, string Result)>();

foreach (var @string in strings)
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
Console.WriteLine($"Success: {successes.Count}");
Console.WriteLine($"Failures: {failures.Count}");
Console.WriteLine();

foreach (var (from, to, result) in failures)
    Console.WriteLine($"Failure from: {from} to: {to} result: {result}");