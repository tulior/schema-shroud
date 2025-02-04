#nullable enable

using System;
using SchemaShroud.AnonymizationStrategies.SchemaShroud.AnonymizationStrategies;

namespace SchemaShroud.AnonymizationStrategies
{
    public sealed class RangeStrategy : IConfigurableAnonymizationStrategy
    {
        public object? Anonymize(object? value) => Anonymize(value, null);

        public object? Anonymize(object? value, SensitiveDataAttribute? attribute)
        {
            if (value == null) return null;

            try
            {
                decimal decimalValue = Convert.ToDecimal(value);
                decimal interval = GetInterval(decimalValue, attribute);
                RoundingMode mode = attribute?.RoundingMode ?? RoundingMode.Floor;

                decimal roundedValue = ApplyRounding(decimalValue, interval, mode);
                return Convert.ChangeType(roundedValue, value.GetType());
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                throw new AnonymizationException($"Failed to anonymize value", ex);
            }
        }

        private decimal GetInterval(decimal value, SensitiveDataAttribute? attribute)
        {
            // Use explicitly configured interval if present
            if (attribute != null && attribute.Interval > 0)
                return (decimal)attribute.Interval;

            // Automatic interval calculation
            decimal absValue = Math.Abs(value);
            if (absValue == 0) return 10;

            int magnitude = (int)Math.Floor(Math.Log10((double)absValue));
            decimal baseInterval = (decimal)Math.Pow(10, magnitude);
            
            return baseInterval < 10 ? 10 : baseInterval;
        }

        private decimal ApplyRounding(decimal value, decimal interval, RoundingMode mode)
        {
            decimal quotient = value / interval;
            return mode switch
            {
                RoundingMode.Floor => Math.Floor(quotient) * interval,
                RoundingMode.Ceiling => Math.Ceiling(quotient) * interval,
                RoundingMode.Round => Math.Round(quotient, MidpointRounding.AwayFromZero) * interval,
                _ => throw new ArgumentOutOfRangeException(nameof(mode))
            };
        }
    }
}