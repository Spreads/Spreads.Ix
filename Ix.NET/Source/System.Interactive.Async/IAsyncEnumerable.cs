﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
#if SPREADS
using Spreads;
#endif

namespace Spreads.Ix {


#if SPREADS

    //public interface IAsyncEnumerable<T> : Spreads.IAsyncEnumerable<T>
    //{

    //}

#else


    /// <summary>
    /// Asynchronous version of the IEnumerable&lt;T&gt; interface, allowing elements of the
    /// enumerable sequence to be retrieved asynchronously.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public interface IAsyncEnumerable<
#if !NO_VARIANCE
out
#endif
        T> : IEnumerable<T>
    {
        /// <summary>
        /// Gets an asynchronous enumerator over the sequence.
        /// </summary>
        /// <returns>Enumerator for asynchronous enumeration over the sequence.</returns>
        IAsyncEnumerator<T> GetEnumerator();
    }
#endif

}
