using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Media_Manager.Extensions
{
    public static class ObservableCollection
    {
        // Extensions
        // =========================================================
        // =========================================================
        public static void AddRange<TSource>(this ObservableCollection<TSource> source, IEnumerable<TSource> items)
        {
            //Loop through elements in items list
            foreach (var item in items)
            {
                //Add current looped item to source
                source.Add(item);
            }
        }
    }
}