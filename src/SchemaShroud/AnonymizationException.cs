using System;
namespace SchemaShroud
{
    public sealed class AnonymizationException : Exception
    {
        public AnonymizationException(string message, Exception inner) 
            : base(message, inner) { }
    }
}