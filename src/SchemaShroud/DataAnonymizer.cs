#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SchemaShroud.AnonymizationStrategies;
using SchemaShroud.AnonymizationStrategies.SchemaShroud.AnonymizationStrategies;

namespace SchemaShroud
{
    /// <summary>
    /// Anonymizes sensitive data in objects based on attributes
    /// </summary>
    public class DataAnonymizer
    {
        private readonly Dictionary<AnonymizationMethod, IAnonymizationStrategy> _strategies;
        private readonly IAnonymizationLogger? _logger;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        /// <summary>
        /// Initialize with custom strategies and logger
        /// </summary>
        public DataAnonymizer(
            Dictionary<AnonymizationMethod, IAnonymizationStrategy>? customStrategies = null,
            IAnonymizationLogger? logger = null)
        {
            _strategies = CreateDefaultStrategies();
            _logger = logger;

            if (customStrategies != null)
            {
                foreach (var kvp in customStrategies)
                {
                    _strategies[kvp.Key] = kvp.Value;
                }
            }
        }

        private static Dictionary<AnonymizationMethod, IAnonymizationStrategy> CreateDefaultStrategies()
        {
            return new Dictionary<AnonymizationMethod, IAnonymizationStrategy>
            {
                {
                    AnonymizationMethod.Hash, new HashStrategy()
                },
                {
                    AnonymizationMethod.Redact, new RedactStrategy()
                },
                {
                    AnonymizationMethod.Mask, new MaskStrategy()
                },
                {
                    AnonymizationMethod.Range, new RangeStrategy()
                }
            };
        }

        /// <summary>
        /// Anonymizes an object and all its nested properties recursively
        /// </summary>
        public T Anonymize<T>(T obj) where T : new()
        {
            return (T)AnonymizeRecursive(obj)!;
        }

        private object? AnonymizeRecursive(object? obj)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            if (type.IsPrimitive || type == typeof(string) || IsCollection(type))
                return obj;

            var anonymized = Activator.CreateInstance(type);
            var properties = GetCachedProperties(type);

            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;

                var originalValue = property.GetValue(obj);
                object? processedValue = ProcessPropertyValue(property, originalValue);
                property.SetValue(anonymized, processedValue);
            }

            return anonymized;
        }

        private object? ProcessPropertyValue(PropertyInfo property, object? value)
        {
            if (value == null) return null;

            var attribute = property.GetCustomAttribute<SensitiveDataAttribute>();
            if (attribute != null)
            {
                return AnonymizeValue(value, attribute);
            }

            if (IsCollection(value.GetType())) return value;

            return !value.GetType().IsPrimitive && value.GetType() != typeof(string)
                ? AnonymizeRecursive(value)
                : value;
        }

        private object? AnonymizeValue(object? value, SensitiveDataAttribute attribute)
        {
            if (!_strategies.TryGetValue(attribute.Method, out var strategy))
            {
                throw new InvalidOperationException(
                    $"No strategy registered for method: {attribute.Method}");
            }

            try
            {
                return strategy switch
                {
                    IConfigurableAnonymizationStrategy configurable =>
                        configurable.Anonymize(value, attribute),
                    _ =>
                        strategy.Anonymize(value)
                };
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Error anonymizing value: {ex.Message}");
                return value;
            }
        }

        private bool IsCollection(Type type)
        {
            return typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
                   && type != typeof(string);
        }

        private PropertyInfo[] GetCachedProperties(Type type)
        {
            return _propertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite)
                    .ToArray());
        }
    }
}