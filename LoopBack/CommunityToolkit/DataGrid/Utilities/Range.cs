// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.WinUI.Utilities
{
    internal class Range<T>(int lowerBound, int upperBound, T value)
    {
        public int Count => UpperBound - LowerBound + 1;

        public int LowerBound { get; set; } = lowerBound;

        public int UpperBound { get; set; } = upperBound;

        public T Value { get; set; } = value;

        public bool ContainsIndex(int index)
        {
            return LowerBound <= index && UpperBound >= index;
        }

        public bool ContainsValue(object value)
        {
            return (Value == null) ? value == null : Value.Equals(value);
        }

        public Range<T> Copy()
        {
            return new Range<T>(LowerBound, UpperBound, Value);
        }
    }
}