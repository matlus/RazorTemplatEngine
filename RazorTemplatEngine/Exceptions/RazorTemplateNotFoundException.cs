using System;
using System.Diagnostics.CodeAnalysis;

namespace RazorTemplatEngine.Exceptions
{

    [Serializable]
    [ExcludeFromCodeCoverage]
    public sealed class RazorTemplateNotFoundException : RazorTemplateEngineException
    {
        public RazorTemplateNotFoundException() { }
        public RazorTemplateNotFoundException(string message) : base(message) { }
        public RazorTemplateNotFoundException(string message, Exception inner) : base(message, inner) { }
        private RazorTemplateNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
