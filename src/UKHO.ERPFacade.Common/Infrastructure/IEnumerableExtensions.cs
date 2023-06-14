﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public static class IEnumerableExtensions
    {
        public static bool AreEquivalent<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            return left.AreEquivalent(right, EqualityComparer<T>.Default);
        }

        public static bool AreEquivalent<T>(this IEnumerable<T> left, IEnumerable<T> right, IEqualityComparer<T> comparer) 
        {
            
            var itemList = left.ToList();
            var otherItemList = right.ToList();
            if(itemList.Count == otherItemList.Count)
            {
                var except = itemList.Except(otherItemList, comparer);

                return !except.Any();
            }

            return false;
        }
    }
}