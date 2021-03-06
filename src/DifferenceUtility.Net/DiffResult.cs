using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DifferenceUtility.Net.Base;
using DifferenceUtility.Net.CollectionUpdateCallbacks;
using DifferenceUtility.Net.Helper;

namespace DifferenceUtility.Net;

/// <summary>
///     <para>This class holds the information about the result of a <see cref="DiffUtil.CalculateDiff{T,T}" /> call.</para>
///     <para>You can consume updates in a DiffResult via <see cref="DispatchUpdatesTo(ObservableCollection{TSource})" />.</para>
/// </summary>
public class DiffResult<TSource, TDestination>
{
    #region Fields
    private readonly TDestination[] _destinationArray;
    private readonly IDiffCallback<TSource, TDestination> _diffCallback;
    private readonly int _moveCount;
    private List<(int From, int Offset)> _offsets;
    private readonly int[] _path;
    private List<(int X, int Y, int OperationId)> _postponedOperations;
    private readonly TSource[] _sourceArray;
    private List<int> _xProcessed, _yProcessed;
    #endregion

    #region Public Methods
    /// <summary>
    ///     Dispatches the update events to the given collection.
    /// </summary>
    /// <param name="observableCollection">
    ///     A collection which is displaying the old collection, and will start displaying the new collection.
    /// </param>
    public void DispatchUpdatesTo([NotNull] ObservableCollection<TSource> observableCollection)
    {
        if (observableCollection is null)
            throw new ArgumentNullException(nameof(observableCollection));

        if (!IsEmpty())
            DispatchUpdatesToCallback(new ObservableCollectionUpdateCallback<TSource, TDestination>(_diffCallback, observableCollection, _destinationArray));
    }

    /// <summary>
    ///     <para>Dispatches update operations to the given callback.</para>
    ///     <para>These updates are atomic such that the first update call affects every update call that comes after it.</para>
    /// </summary>
    /// <param name="updateCallback">The callback to receive the update operations.</param>
    public void DispatchUpdatesTo([NotNull] ICollectionUpdateCallback updateCallback)
    {
        if (updateCallback is null)
            throw new ArgumentNullException(nameof(updateCallback));

        if (!IsEmpty())
            DispatchUpdatesToCallback(updateCallback);
    }

    /// <summary>
    ///     Creates a new array containing the calculated path for applying the diff result. See <see cref="DiffOperation" />
    ///     for decoding.
    /// </summary>
    public int[] GetPath()
    {
        return _path.ToArray();
    }
    #endregion

    #region Constructors
    internal DiffResult(IDiffCallback<TSource, TDestination> diffCallback, TSource[] sourceArray, TDestination[] destinationArray, int[] path, int moveCount = 0)
    {
        _destinationArray = destinationArray;
        _diffCallback = diffCallback;
        _moveCount = moveCount;
        _path = path;
        _sourceArray = sourceArray;
    }
    #endregion

    #region Private Methods
    private void CreateXOffset(int from, bool increment, int operationId)
    {
        var offset = increment ? 1 : -1;

        for (var i = 0; i < _offsets.Count; i++)
        {
            var queryOffset = _offsets[i];

            // The provided offset should be applied to offsets that are positioned after the provided 'from' position.
            if (queryOffset.From > from)
                _offsets[i] = (queryOffset.From + offset, queryOffset.Offset);
        }

        // Finally, create the offset.
        // We use this technique so that the list remains sorted. This allows us to
        // use a regular List instead of SortedList, which carries a performance hit.
        if (operationId > _offsets.Count - 1)
            _offsets.Add((from, offset));

        else
            _offsets.Insert(operationId, (from, offset));
    }

    private void DispatchUpdatesToCallback(ICollectionUpdateCallback updateCallback)
    {
        if (updateCallback is not BatchingCollectionUpdateCallback batchingCallback)
            batchingCallback = new BatchingCollectionUpdateCallback(updateCallback);

        if (_offsets is null)
        {
            // Provide capacity to avoid re-allocations. There will never be more offsets than items in the path.
            _offsets = new List<(int From, int Offset)>(_path.Length);
        }
        else
            _offsets.Clear();

        _xProcessed ??= new List<int>(_path.Length);

        // Postponed operations only required if there are moves in the path.
        if (_moveCount > 0)
        {
            if (_postponedOperations is null)
                _postponedOperations = new List<(int, int, int)>(_moveCount);

            else
                _postponedOperations.Clear();

            _yProcessed ??= new List<int>(_moveCount);
        }

        var currentX = -1;
        var currentY = -1;

        for (var operationId = 0; operationId < _path.Length; operationId++)
        {
            var operation = _path[operationId];

            (int X, int Y, int OperationId) postponedOperation;

            // Vertical movement.
            if ((operation & DiffOperation.Insert) != 0)
            {
                currentY++;

                // Regular insertion if no move flag.
                if ((operation & DiffOperation.Move) == 0)
                {
                    var offsetY = OffsetY(currentY);

                    batchingCallback.OnInserted(offsetY, currentY, 1);

                    CreateXOffset(offsetY, true, operationId);

                    continue;
                }

                // X coordinate encoded in payload.
                var x = operation >> DiffOperation.Offset;

                // Postpone the current operation if an operation with matching coordinates hasn't already been postponed.
                if (!TryFindPostponedOperation(x, currentY, out postponedOperation))
                {
                    _postponedOperations.Add((x, currentY, operationId));

                    continue;
                }
            }
            // Horizontal movement.
            else if ((operation & DiffOperation.Remove) != 0)
            {
                currentX++;

                // Regular removal if no move flag.
                if ((operation & DiffOperation.Move) == 0)
                {
                    var offsetX = OffsetX(currentX);

                    batchingCallback.OnRemoved(offsetX, 1);

                    CreateXOffset(offsetX, false, operationId);

                    continue;
                }

                // Y coordinate encoded in payload.
                var y = operation >> DiffOperation.Offset;

                // Try to find an existing postponed operation with the provided coordinates.
                if (!TryFindPostponedOperation(currentX, y, out postponedOperation))
                {
                    _postponedOperations.Add((currentX, y, operationId));

                    continue;
                }
            }
            // If the operation is neither remove nor insert, assume diagonal.
            else
            {
                // Diagonal operations don't contain any coordinates, so just increment normally.
                currentX++;
                currentY++;

                if ((operation & DiffOperation.Update) != 0)
                    batchingCallback.OnChanged(OffsetX(currentX), currentY, 1);

                continue;
            }

            _postponedOperations.Remove(postponedOperation);

            // Apply offsets.
            var offsetPostponedX = OffsetX(postponedOperation.X);
            var offsetPostponedY = OffsetY(postponedOperation.Y);

            if ((operation & DiffOperation.Update) != 0)
                batchingCallback.OnChanged(offsetPostponedX, postponedOperation.Y, 1);

            batchingCallback.OnMoved(offsetPostponedX, offsetPostponedY);

            // Create the offsets as a result of the move operation.
            if (offsetPostponedX > offsetPostponedY)
            {
                CreateXOffset(offsetPostponedY, true, postponedOperation.OperationId);
                CreateXOffset(offsetPostponedX + 1, false, operationId);
            }
            else if (offsetPostponedX < offsetPostponedY)
            {
                CreateXOffset(offsetPostponedX, false, postponedOperation.OperationId);
                CreateXOffset(offsetPostponedY, true, operationId);
            }
        }

        batchingCallback.DispatchLastEvent();
    }

    private bool IsEmpty()
    {
        // Path, diff callback, source array, and destination array cannot be null however,
        // the source or destination arrays can be empty, as long as they're not both empty.
        return _path is not { Length: > 0 }
            || _diffCallback is null
            || _sourceArray is null
            || _destinationArray is null
            || _sourceArray.Length == 0 && _destinationArray.Length == 0;
    }

    private int OffsetX(int x)
    {
        if (_offsets.Count == 0)
            return x;

        _xProcessed.Clear();

        for (var i = 0; i < _offsets.Count; i++)
        {
            if (i < _xProcessed.Count && _xProcessed[i] == i)
                continue;

            var offset = _offsets[i];

            if (x < offset.From)
                continue;

            x += offset.Offset;

            // By adding and inserting like this, we can keep the list sorted, meaning
            // that querying it at the start of the loop can be faster. Rather than
            // using List.Contains(), which is an O(n) operation, we can retrieve the
            // item directly via its index, which is an O(1) operation.
            if (i > _xProcessed.Count - 1)
                _xProcessed.Add(i);

            else
                _xProcessed.Insert(i, i);

            // The loop will increment 'i' before the next loop runs, so we need to set it to -1 in order to start again from 0.
            i = -1;
        }

        return x;
    }

    private int OffsetY(int y)
    {
        // Postponed operations will be null if there are no moves in the provided path.
        // Y will also not change if there are no postponed updates.
        if (_postponedOperations is not { Count: > 0 })
            return y;

        _yProcessed.Clear();

        // The Y offset needs to be kept separate as we need the unmodified Y coordinate for the query.
        var yOffset = 0;

        for (var i = 0; i < _postponedOperations.Count; i++)
        {
            if (i < _yProcessed.Count && _yProcessed[i] == i)
                continue;

            var postponedOperation = _postponedOperations[i];

            if (postponedOperation.Y == y)
                continue;

            var offsetOperationX = OffsetX(postponedOperation.X);

            var yWithOffset = y + yOffset;

            if (postponedOperation.Y < y && offsetOperationX >= yWithOffset)
                yOffset--;

            else if (postponedOperation.Y > y && offsetOperationX <= yWithOffset)
                yOffset++;

            else
                continue;

            // By adding and inserting like this, we can keep the list sorted, meaning
            // that querying it at the start of the loop can be faster. Rather than
            // using List.Contains(), which is an O(n) operation, we can retrieve the
            // item directly via its index, which is an O(1) operation.
            if (i > _yProcessed.Count - 1)
                _yProcessed.Add(i);

            else
                _yProcessed.Insert(i, i);

            // The loop will increment 'i' before the next loop runs, so we need to set it to -1 in order to start again from 0.
            i = -1;
        }

        return y + yOffset;
    }

    private bool TryFindPostponedOperation(int x, int y, out (int, int, int) postponedOperation)
    {
        for (var postponedOperationIndex = 0; postponedOperationIndex < _postponedOperations.Count; postponedOperationIndex++)
        {
            var operation = _postponedOperations[postponedOperationIndex];

            if (operation.X != x || operation.Y != y)
                continue;

            postponedOperation = operation;

            return true;
        }

        postponedOperation = (-1, -1, -1);

        return false;
    }
    #endregion

    #region Static Fields
    private static DiffResult<TSource, TDestination> _emptyDiffResult;
    #endregion

    #region Internal Static Methods
    /// <summary>
    ///     Gets an empty <see cref="DiffResult{TOld,TNew}" /> that takes zero action when applied.
    /// </summary>
    internal static DiffResult<TSource, TDestination> Empty()
    {
        return _emptyDiffResult ??= new DiffResult<TSource, TDestination>(null, null, null, null);
    }

    /// <summary>
    ///     Gets a configured <see cref="DiffResult{TOld,TNew}" /> that ignores diagonals.
    /// </summary>
    internal static DiffResult<TSource, TDestination> NoDiagonals(IDiffCallback<TSource, TDestination> diffCallback, TSource[] oldArray, TDestination[] newArray)
    {
        var path = new int[oldArray.Length + newArray.Length];

        // Add X/remove operations first.
        for (var x = 0; x < oldArray.Length; x++)
            path[x] = (x << DiffOperation.Offset) | DiffOperation.Remove;

        // Then add Y/insert operations.
        for (var y = 0; y < newArray.Length; y++)
            path[y + oldArray.Length] = (y << DiffOperation.Offset) | DiffOperation.Insert;

        return new DiffResult<TSource, TDestination>(diffCallback, oldArray, newArray, path);
    }
    #endregion
}