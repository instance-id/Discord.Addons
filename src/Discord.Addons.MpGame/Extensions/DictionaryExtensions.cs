﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.Addons.MpGame
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dictionary.TryGetValue(key, out var ret) ? ret : defaultValue;
        }

        //public static void Decontstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue val)
        //{
        //    key = kvp.Key;
        //    val = kvp.Value;
        //}
    }
}