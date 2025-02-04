#nullable enable

namespace SchemaShroud.AnonymizationStrategies
{
    /// <summary>
    /// Masks strings showing only last 4 characters
    /// </summary>
    public sealed class MaskStrategy : IAnonymizationStrategy
    {
        public object? Anonymize(object? value)
        {
            if (value == null) return null;
            
            var stringValue = value.ToString()!;
            if (stringValue.Length < 4)
            {
                return new RedactStrategy().Anonymize(value);
            }

            return new string('*', stringValue.Length - 4) 
                   + stringValue.Substring(stringValue.Length - 4);
        }
    }
}