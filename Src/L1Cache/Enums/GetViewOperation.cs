namespace L1CacheSystem.Enums;

public enum GetViewOperation : byte
{
    /// <summary>
    /// Do nothing with the corresponding column.
    /// </summary>
    None = 0,

    /// <summary>
    /// It will always call the corresponding function, passing the column values to it.
    /// </summary>
    /// <remarks>This is the responsibility that calls the function's calculation side. The system expects it to be != null.</remarks>
    TryGet = 1
}