namespace L1CacheSystem.Enums;

public enum ValueOwnership
{
    /// <summary>
    /// Ownership not transferred
    /// </summary>
    /// <remarks>Before placing an item into the cache, a copy must be made; the system will handle this automatically.</remarks>
    NotTransferred = 0,

    /// <summary>
    /// Ownership not transferred
    /// </summary>
    /// <remarks>The caller explicitly stated that they did not save the reference anywhere, so the value can be placed in the cache without performing a copy.</remarks>
    Transferred = 1
}