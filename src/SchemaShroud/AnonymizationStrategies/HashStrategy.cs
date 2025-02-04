#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;

namespace SchemaShroud.AnonymizationStrategies
{
    /// <summary>
    /// Hashes string values using SHA256
    /// </summary>
    public sealed class HashStrategy : IAnonymizationStrategy
    {
        public object? Anonymize(object? value)
        {
            if (value == null) return null;
            
            var stringValue = value.ToString()!;
            using var sha256 = SHA256.Create();
            
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
            return BitConverter.ToString(hashedBytes)
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}