using System;
using System.Collections.Generic;
using System.Linq;

namespace ModestTree
{
    public static class MiscExtensions
    {
        // We'd prefer to use the name Format here but that conflicts with
        // the existing string.Format method
        public static string Fmt(this string s, params object[] args)
        {
            return String.Format(s, args);
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            return string.Join(separator, values.ToArray());
        }

        // When using C# 4.6, for some reason the normal AddRange causes some allocations
        // https://issuetracker.unity3d.com/issues/dot-net-4-dot-6-unexpected-gc-allocations-in-list-dot-addrange
        public static void AllocFreeAddRange<T>(this IList<T> list, IList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                list.Add(items[i]);
            }
        }

        // Most of the time when you call remove you always intend on removing something
        // so assert in that case
        public static void RemoveWithConfirm<T>(this IList<T> list, T item)
        {
            bool removed = list.Remove(item);
            Assert.That(removed);
        }

        public static void RemoveWithConfirm<T>(this LinkedList<T> list, T item)
        {
            bool removed = list.Remove(item);
            Assert.That(removed);
        }

        public static void RemoveWithConfirm<TKey, TVal>(this IDictionary<TKey, TVal> dictionary, TKey key)
        {
            bool removed = dictionary.Remove(key);
            Assert.That(removed);
        }

        public static void RemoveWithConfirm<T>(this HashSet<T> set, T item)
        {
            bool removed = set.Remove(item);
            Assert.That(removed);
        }

        public static TVal GetValueAndRemove<TKey, TVal>(this IDictionary<TKey, TVal> dictionary, TKey key)
        {
            TVal val = dictionary[key];
            dictionary.RemoveWithConfirm(key);
            return val;
        }
    }
}
