using System;
using System.Collections.Generic;
using System.Text;

namespace Hubble.Core.Cache
{
    internal interface IManagedCache
    {
        long CacheSize { get; }

        /// <summary>
        /// Reduce Memory
        /// </summary>
        /// <param name="percentage">Reduce cache memory to the percentage of original cache size</param>
        void ReduceMemory(int percentage);
    }
}
