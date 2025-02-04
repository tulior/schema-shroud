# SchemaShroud C# Data Anonymizer

[![MIT License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**Secure Data Obfuscation for .NET Applications**

SchemaShroud provides targeted data anonymization for C# objects through declarative attributes. Designed for developers handling sensitive information in non-production environments.

**Core Features**

- Attribute-based field marking
- Four anonymization strategies
- Recursive object processing
- Custom strategy support

**Installation**

1. Clone repository
2. Include these files in your project:
    - DataAnonymizer.cs
    - SensitiveDataAttribute.cs
    - AnonymizationStrategies/**

**Usage**

1. Annotate model properties:

```csharp
public class CustomerRecord
{
    [SensitiveData(AnonymizationMethod.Hash)]
    public string Email { get; set; }

    [SensitiveData(AnonymizationMethod.Mask)]
    public string PaymentCard { get; set; }

    [SensitiveData(AnonymizationMethod.Range, Interval = 1000)]
    public int Salary { get; set; }
}
```

2. Process objects:

```csharp
var processor = new DataAnonymizer();
var cleanData = processor.Anonymize(rawData);
```

**Anonymization Methods**

| Method   | Behavior                             | Example               |
|----------|--------------------------------------|-----------------------|
| Hash     | SHA256 conversion                    | "test@x.com" → hex    |
| Redact   | Fixed replacement ("***")            | "John Doe" → "***"    |
| Mask     | Last 4 visible, others starred       | "4111111111111111" → "************1111" |
| Range    | Rounds to nearest interval           | 1234 → 1000 (interval=1000) |

**Customization**

Extend strategies by implementing `IAnonymizationStrategy`:

```csharp
public class NoiseStrategy : IAnonymizationStrategy
{
    public object Anonymize(object value)
    {
        return (int)value + new Random().Next(-100, 100);
    }
}

// Configure:
var strategies = new Dictionary<AnonymizationMethod, IAnonymizationStrategy>
{
    { AnonymizationMethod.Range, new NoiseStrategy() }
};
var processor = new DataAnonymizer(strategies);
```

**Requirements**

- .NET Standard 2.0+
- No external dependencies

**Limitations**

- Requires mutable properties
- Primarily handles string/numeric types
- No built-in collection support

**Contributing**

Issue reports and pull requests welcome. Maintain test coverage when submitting changes.

**License**

MIT - see [LICENSE](LICENSE) file