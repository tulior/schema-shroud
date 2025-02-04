namespace SchemaShroud.AnonymizationStrategies
{
    public interface IAnonymizationStrategy
    {
        object Anonymize(object value);
    }
}