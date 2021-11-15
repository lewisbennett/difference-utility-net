using DifferenceUtility.Net;
using Sample.Assets;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

            data.CollectionChanged += DataOnCollectionChanged;
            
            // Request collection of people.
            var people = await API.GetPeopleAsync();

            // Calculate the difference between the new data, and the existing data.
            // var result = DiffUtil.CalculateDiff(data, people, callback);
            //
            // // Apply the changes to the data.
            // result.DispatchUpdatesTo(data);

            foreach (var person in people)
                data.Add(person);

            Console.WriteLine();
            
            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.WriteLine();

            // Some time has passed - the remote data has changed.
            await Task.Delay(TimeSpan.FromSeconds(2));

            // Get the new data.
            people = await API.GetPeopleAsync();

            var result = DiffUtil.CalculateDiff(data, people, callback);
            
            result.DispatchUpdatesTo(data);

            Console.WriteLine();
            
            foreach (var item in data)
                Console.WriteLine(item.ToString());

            Console.ReadLine();
        }

        private static void DataOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        }
    }
}
