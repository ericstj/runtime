using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace System.Text.Json.Generator
{
    [Generator]
    public class JsonGenerator : ISourceGenerator
    {
        public void Execute(SourceGeneratorContext context)
        {
            // TODO - actual source generator goes here!
        }

        public void Initialize(InitializationContext context)
        {
            // No initialization required for this one
        }
    }
}
