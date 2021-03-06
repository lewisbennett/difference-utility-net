namespace DifferenceUtility.Net.Helper;

/// <summary>
///     <para>Defines a set of flags for encoding the steps within a diff result.</para>
///     <para>
///         Query <see cref="Remove" />, <see cref="Insert" />, <see cref="Move" />, and <see cref="Update" /> flags with
///         bitwise AND operations:
///         <c>var isInsert = (payload &amp; DiffOperation.Insert) != 0;</c>
///     </para>
///     <para>
///         If the payload has either the <see cref="Remove" /> or <see cref="Insert" /> flag, you can bitshift it to
///         extract the X or Y coordinate
///         (index within source or destination collection) respectively. If however, the payload also contains the
///         <see cref="Move" /> flag, the coordinate
///         that is encoded will be inverted (i.e. <see cref="Remove" /> will contain the Y coordinate and
///         <see cref="Insert" /> will contain the X coordinate):
///         <c>var coordinate = payload &gt;&gt; DiffOperation.Offset;</c>
///     </para>
///     <para>
///         No payload will contain the <see cref="Move" /> flag if move detection is disabled when calculating the diff
///         result.
///     </para>
///     <para>
///         A payload with a value of <c>0</c> represents a step where no action should be taken. A payload with neither
///         the <see cref="Remove" /> nor
///         <see cref="Insert" /> flag, yet has the <see cref="Update" /> flag, represents a step where the contents of the
///         item are due to change, but the
///         item remains in the same position. These payloads contain no encoded coordinate however, both the required X
///         and Y coordinates (position of
///         existing item in the source collection and position of the updated item in the destination collection) can be
///         calculated using the steps prior.
///     </para>
/// </summary>
public static class DiffOperation
{
    #region Constant Values
    /// <summary>
    ///     Represents an insert operation.
    /// </summary>
    public const int Insert = 1;

    /// <summary>
    ///     Represents a remove operation.
    /// </summary>
    public const int Remove = Insert << 1;

    /// <summary>
    ///     Represents a move operation.
    /// </summary>
    public const int Move = Remove << 1;

    /// <summary>
    ///     The number of bits to offset a coordinate value from its operation flags.
    /// </summary>
    public const int Offset = 4;

    /// <summary>
    ///     Represents an update operation.
    /// </summary>
    public const int Update = Move << 1;
    #endregion
}