namespace L1CacheSystem.Enums;

public enum GetViewSource : byte
{
    /// <summary>
    /// A source is automatically created during retrieval by calling the ‘FuncCreateNewSource’ delegate.
    /// </summary>
    ByFunc = 0,

    /// <summary>
    /// The source is set via an external ‘SetSource’ call.
    /// </summary>
    /// <remarks></remarks>
    External = 1
}