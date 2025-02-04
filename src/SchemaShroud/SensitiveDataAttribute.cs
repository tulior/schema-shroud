#nullable enable

using System;

namespace SchemaShroud
{
    public enum AnonymizationMethod
    {
        Hash,
        Redact,
        Mask,
        Range
    }

    public enum RoundingMode
    {
        Floor,
        Ceiling,
        Round
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SensitiveDataAttribute : Attribute
    {
        public AnonymizationMethod Method { get; }
        public double Interval { get; set; }
        public RoundingMode RoundingMode { get; set; } = RoundingMode.Floor;

        public SensitiveDataAttribute(AnonymizationMethod method)
        {
            Method = method;
        }
    }
}