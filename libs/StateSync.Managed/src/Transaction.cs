﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.MixedReality.Sharing.StateSync
{
    public enum TransactionResult
    {
        Succeeded,
        Conflict,
        Failed
    }

    /// <summary>
    /// Transactions are used to mutate the storage, adding/updated/removing values for keys.
    /// </summary>
    public class Transaction : DisposablePointerBase
    {
        public Transaction(IntPtr transactionPointer)
            : base(transactionPointer)
        {
        }

        /// <summary>
        /// Specifies a key as an immutable constraints in order for this transaction to be commited succesfully.
        /// If the value for the key changes, this transaction will be rejected.
        /// </summary>
        /// <param name="key">The required key.</param>
        public void Require(RefKey key)
        {
            ThrowIfDisposed();

            StateSyncAPI.Transaction_Require(Pointer, key.Pointer);
        }

        /// <summary>
        /// Adds a value to be applied to the storage with a given key and sub key combination.
        /// </summary>
        /// <param name="key">Key to associate the value with.</param>
        /// <param name="subKey">Sub key to associate the value with.</param>
        /// <param name="value">The binary value.</param>
        public void Set(RefKey key, ulong subKey, ReadOnlySpan<byte> value)
        {
            if (value.Length == 0)
            {
                throw new ArgumentException("Given value is empty, if you intend to clear a value associated with key and sub key, use Clear.");
            }

            ThrowIfDisposed();

            StateSyncAPI.Transaction_Set(Pointer, key.Pointer, subKey, value);
        }

        /// <summary>
        /// Clears (removes) a value associated wiht a key and sub key.
        /// </summary>
        /// <param name="key">Key for which to remove the associated value.</param>
        /// <param name="subKey">Sub key for which to remove the associated value.</param>
        public void Clear(RefKey key, ulong subKey)
        {
            ThrowIfDisposed();

            StateSyncAPI.Transaction_Clear(Pointer, key.Pointer, subKey);
        }

        /// <summary>
        /// Asynchronously commit the transaction, returning the <see cref="TransactionResult"/> on completion.
        /// </summary>
        /// <returns>The awaitable task with result of the transaction.</returns>
        public async Task<TransactionResult> CommitAsync()
        {
            ThrowIfDisposed();

            TaskCompletionSource<TransactionResult> tcs = new TaskCompletionSource<TransactionResult>();

            StateSyncAPI.Transaction_Commit(Pointer, tcs.SetResult);

            return await tcs.Task;
        }

        protected override void ReleasePointer(IntPtr pointer)
        {
            ThrowIfDisposed();

            StateSyncAPI.Transaction_Release(pointer);
        }
    }
}
