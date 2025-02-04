#nullable enable

namespace SchemaShroud.AnonymizationStrategies
{
    /// <summary>
    /// Redacts string values with ***
    /// </summary>
    public sealed class RedactStrategy : IAnonymizationStrategy
    {
        public object? Anonymize(object? value)
        {
            return value == null ? null : "***";
        }
    }
}