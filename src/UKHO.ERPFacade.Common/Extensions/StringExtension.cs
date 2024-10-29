﻿using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class StringExtension
    {
        public static string ToSubstring(string value, int startIndex, int length)
        {
            return value.Substring(startIndex, Math.Min(length, value.Length));
        }
    }
}