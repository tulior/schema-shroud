#nullable enable

namespace SchemaShroud
{
    /// <summary>
    /// Logger interface for anonymization operations
    /// </summary>
    public interface IAnonymizationLogger
    {
        void LogWarning(string message);
    }
}