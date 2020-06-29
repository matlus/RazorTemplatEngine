using System;
using System.Diagnostics.CodeAnalysis;

namespace RazorTemplatEngine.Exceptions
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class RazorTemplateEngineException : Exception
    {
        public RazorTemplateEngineException() { }
        public RazorTemplateEngineException(string message) : base(message) { }
        public RazorTemplateEngineException(string message, Exception inner) : base(message, inner) { }
        protected RazorTemplateEngineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
