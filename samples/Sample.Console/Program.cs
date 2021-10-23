using DifferenceUtility.Net;
using Sample.Assets;
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
            var people = await API.GetPeopleAsync();

            // Calculate the difference between the new data, and the existing data.
            var result = DiffUtil.CalculateDiff(data, people, callback);

            // Apply the changes to the data.
            result.Apply(data);

            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.WriteLine();

            // Some time has passed - the remote data has changed.
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Get the new data.
            people = await API.GetPeopleAsync();

            result = DiffUtil.CalculateDiff(data, people, callback);

            result.Apply(data);

            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.ReadLine();
        }
    }
}
