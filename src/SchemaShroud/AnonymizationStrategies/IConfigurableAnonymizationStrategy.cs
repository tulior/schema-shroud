namespace SchemaShroud.AnonymizationStrategies
{
#nullable enable

    namespace SchemaShroud.AnonymizationStrategies
    {
        public interface IConfigurableAnonymizationStrategy : IAnonymizationStrategy
        {
            object? Anonymize(object? value, SensitiveDataAttribute attribute);
        }
    }
}