namespace L1CacheSystem.Enums;

public enum SetViewOperation : byte
{
    /// <summary>
    /// Do nothing with the corresponding column.
    /// </summary>
    None = 0,

    /// <summary>
    /// It will definitely call the paired function to get the column value and set the value.
    /// </summary>
    /// <remarks>This is the responsibility that calls the function's calculation side. The system expects it to be != null.</remarks>
    Set = 1,

    /// <summary>
    /// Removes the column value if it exists.
    /// </summary>
    RemoveIfExist = 2
}