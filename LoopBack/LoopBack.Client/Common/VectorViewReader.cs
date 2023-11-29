﻿using System.Collections;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace LoopBack.Client.Common
{
    /// <summary>
    /// The reader of <see cref="IVectorView{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of the elements of <paramref name="List"/>.</typeparam>
    /// <param name="Source">The <typeparamref name="IVectorView{TSource}"/> to be redden.</param>
    public record class VectorViewReader<T>(IReadOnlyList<T> Source) : IReadOnlyList<T>
    {
        /// <inheritdoc/>
        public T this[int index] => Source[index];

        /// <inheritdoc/>
        public int Count => Source.Count;

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Source.Count; i++)
            {
                yield return Source[i];
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
