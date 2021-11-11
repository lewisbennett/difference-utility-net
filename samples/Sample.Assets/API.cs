using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Assets
{
    // Just a simple class to emulate the loading of data from an external API source.

    public static class API
    {
        private static bool _hasLoadedPageOne;

        public static async Task<Person[]> GetPeopleAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            var toReturn = _hasLoadedPageOne ? SecondDataset : FirstDataset;

            _hasLoadedPageOne = !_hasLoadedPageOne;

            return toReturn.OrderBy(x => x.ID).ToArray();
        }

        private static readonly Person[] FirstDataset = new[]
            {
                new Person
                {
                    FirstName = "John",
                    LastName = "Smith",
                    ID = 1
                },
                new Person
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    ID = 2
                },
                new Person
                {
                    FirstName = "Clark",
                    LastName = "Kent",
                    ID = 3
                },
                new Person
                {
                    FirstName = "Joe",
                    LastName = "Blogs",
                    ID = 4
                }
            };

        private static readonly Person[] SecondDataset = new[]
            {
                new Person
                {
                    FirstName = "Lois",
                    LastName = "Lane",
                    ID = 5
                },
                new Person
                {
                    FirstName = "Joey",
                    LastName = "Blogs",
                    ID = 4
                },
                new Person
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    ID = 2
                },
                new Person
                {
                    FirstName = "Barry",
                    LastName = "Allen",
                    ID = 6
                },
                new Person
                {
                    FirstName = "John",
                    LastName = "Smithy",
                    ID = 1
                }
            };
    }
}
