using System;
using System.Threading.Tasks;

namespace Sample.Assets;

// Just a simple class to emulate the loading of data from an external API source.

public static class API
{
    private static bool _hasLoadedPageOne;

    public static async Task<Person[]> GetPeopleAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var toReturn = _hasLoadedPageOne ? SecondDataset : FirstDataset;

        _hasLoadedPageOne = !_hasLoadedPageOne;

        return toReturn;
    }

    private static readonly Person[] FirstDataset =
    {
        new()
        {
            FirstName = "John",
            LastName = "Smith",
            ID = 1
        },
        new()
        {
            FirstName = "Jane",
            LastName = "Doe",
            ID = 2
        },
        new()
        {
            FirstName = "Clark",
            LastName = "Kent",
            ID = 3
        },
        new()
        {
            FirstName = "Joe",
            LastName = "Blogs",
            ID = 4
        }
    };

    private static readonly Person[] SecondDataset =
    {
        new()
        {
            FirstName = "Lois",
            LastName = "Lane",
            ID = 5
        },
        new()
        {
            FirstName = "Joey",
            LastName = "Blogs",
            ID = 4
        },
        new()
        {
            FirstName = "Jane",
            LastName = "Doe",
            ID = 2
        },
        new()
        {
            FirstName = "Barry",
            LastName = "Allen",
            ID = 6
        },
        new()
        {
            FirstName = "John",
            LastName = "Smithy",
            ID = 1
        }
    };
}