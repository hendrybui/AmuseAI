using System.Collections.Generic;
using System.Linq;

namespace Amuse.UI.Helpers
{
    public record SelectItem<T>
    {
        public T Item { get; set; }
        public bool IsSelected { get; set; }
    }

    public static class SelectItemExt
    {
        public static List<SelectItem<T>> ToSelectList<T>(this IEnumerable<T> collection, IEnumerable<T> selections = default)
        {
            return collection.Select(x => new SelectItem<T>
            {
                Item = x,
                IsSelected = selections?.Contains(x) == true
            }).ToList();
        }


        public static List<T> ToSelectedList<T>(this IEnumerable<SelectItem<T>> collection)
        {
            return collection
                .Where(x => x.IsSelected)
                .Select(x => x.Item)
                .ToList();
        }
    }
}
