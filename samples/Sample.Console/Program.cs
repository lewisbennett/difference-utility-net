using DifferenceUtility.Net;
using Sample.Assets;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.NetConsole
{
    public static class Program
    {
        public class CharWrapper
        {
            public char C { get; set; }
        }
        
        // Main application entry point.
        public static async Task Main()
        {
            // badfhlocz
            // azhbdfc
            
            // nwpalgfbds
            // mlfsgbadnhzx
            
            // var source = new ObservableCollection<CharWrapper>("nwpalgfbds".Select(x => new CharWrapper
            // {
            //     C = x
            // }));
            //
            // var destination = "mlfsgbadnhzx".Select(x => new CharWrapper
            // {
            //     C = x
            // });
            //
            // source.CollectionChanged += DataOnCollectionChanged;
            //
            // var c = new TestDiffCallback();
            //
            // var r = DiffUtil.CalculateDiff(source, destination, c);
            //
            // r.DispatchUpdatesTo(source);
            //
            // foreach (var x in source)
            // {
            //     Console.Write(x.C);
            // }
            //
            // return;
            
            var data = new ObservableCollection<Person>();
            var callback = new PersonDiffCallback();
            
            data.CollectionChanged += DataOnCollectionChanged;
            
            // Request collection of people.
            var people = await API.GetPeopleAsync();
            
            // Calculate the difference between the new data, and the existing data.
            var result = DiffUtil.CalculateDiff(data, people, callback);
            
            // Apply the changes to the data.
            result.DispatchUpdatesTo(data);
            
            Console.WriteLine();
            
            foreach (var item in data)
                Console.WriteLine(item.ToString());
            
            Console.WriteLine();
            
            // Some time has passed - the remote data has changed.
            await Task.Delay(TimeSpan.FromSeconds(2));
            
            // Get the new data.
            people = await API.GetPeopleAsync();
            
            result = DiffUtil.CalculateDiff(data, people, callback);
            
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
