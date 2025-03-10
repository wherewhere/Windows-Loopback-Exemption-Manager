using LoopBack.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace LoopBack.Common
{
    public static class Enumerable
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ICollection{TSource}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection.</typeparam>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">The <typeparamref name="TCollection"/> to be added.</param>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="ICollection{TSource}"/>.
        /// The collection itself cannot be <see langword="null"/>, but it can contain elements that are
        /// <see langword="null"/>, if type <typeparamref name="TSource"/> is a reference type.</param>
        /// <param name="dispatcher">The target <see cref="CoreDispatcher"/> to invoke the code on.</param>
        /// <returns>A <see cref="Task"/> which represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="collection"/> is null.</exception>
        public static async Task AddRangeAsync<TCollection, TSource>(this TCollection source, IEnumerable<TSource> collection, CoreDispatcher dispatcher) where TCollection : ICollection<TSource>, INotifyCollectionChanged
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(collection);

            if (source is List<TSource> list)
            {
                await dispatcher.ResumeForegroundAsync();
                list.AddRange(collection);
            }
            else if (source is TSource[] array)
            {
                int count = collection.Count();
                if (count > 0)
                {
                    int _size = Array.FindLastIndex(array, (x) => x != null) + 1;
                    if (array.Length - _size < count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(source));
                    }

                    await dispatcher.ResumeForegroundAsync();
                    if (collection is ICollection<TSource> c)
                    {
                        c.CopyTo(array, _size);
                    }
                    else
                    {
                        foreach (TSource item in collection)
                        {
                            array[_size++] = item;
                        }
                    }
                }
            }
            else if (source is ISet<TSource> set)
            {
                await dispatcher.ResumeForegroundAsync();
                set.UnionWith(collection);
            }
            else
            {
                foreach (TSource item in collection)
                {
                    await dispatcher.AwaitableRunAsync(() => source.Add(item));
                }
            }
        }
    }
}
