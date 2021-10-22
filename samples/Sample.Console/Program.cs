using DifferenceUtility.Net;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sample.NetConsole
{
    public static class Program
    {
        // Main application entry point.
        public static async Task Main()
        {
            var data = new ObservableCollection<Person>();
            var callback = new PersonDiffCallback();

            // Request collection of people.
            var people = await GetPeopleAsync();

            // Calculate the difference between the new data, and the existing data.
            var result = DiffUtil.CalculateDiff(data, people, callback);

            // Apply the changes to the data.
            result.Apply(data);

            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.WriteLine();

            // Some time has passed - the "remote" data has changed.
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Get the new data.
            people = await GetPeopleAgainAsync();

            result = DiffUtil.CalculateDiff(data, people, callback);

            result.Apply(data);

            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.ReadLine();
        }

        // Example method in place of API call.
        private static async Task<Person[]> GetPeopleAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            return new[]
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
        }

        // Example method in place of API call after data has changed.
        private static async Task<Person[]> GetPeopleAgainAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            return new[]
            {
                new Person
                {
                    FirstName = "John",
                    LastName = "Smithy",
                    ID = 1
                },
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
                }
            };
        }
    }
}
