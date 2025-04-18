using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.MemoryProfilerExtension.Editor
{
    static class IEnumeratorExtensions
    {
        public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> enumerator) => enumerator;
    }
}
