// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace SourceGenerators
{
    [EventSource(Name = "Microsoft-NET-SourceGeneration")]
    public sealed class SourceGeneratorsEventSource : EventSource
    {
        public static readonly SourceGeneratorsEventSource Log = new SourceGeneratorsEventSource();

        public static class Keywords
        {
            public const EventKeywords Performance = (EventKeywords)1;
        }
        public static class Tasks
        {
            public const EventTask GeneratorPhaseRunTime = (EventTask)1;
        }

        private SourceGeneratorsEventSource() { }

        [NonEvent]
        public SourceGeneratorsOperation StartGeneratorPhase(string generatorName, string assemblyPath, string phase)
        {
            SourceGeneratorsOperation operation = new(generatorName, assemblyPath, phase, Guid.NewGuid().ToString(), Stopwatch.GetTimestamp());
            StartGeneratorPhase(operation.generatorName, operation.assemblyPath, operation.phase, operation.id);
            return operation;
        }

        [Event(1, Keywords = Keywords.Performance, Level = EventLevel.Informational, Opcode = EventOpcode.Start, Task = Tasks.GeneratorPhaseRunTime)]
        private void StartGeneratorPhase(string generatorName, string assemblyPath, string phase, string id) =>
            WriteEvent(1, generatorName, assemblyPath, phase, id);

        [NonEvent]
        public void StopGeneratorPhase(SourceGeneratorsOperation operation) =>
            StopGeneratorPhase(operation.generatorName, operation.assemblyPath, operation.phase, Stopwatch.GetTimestamp() - operation.startTicks, operation.id);

        [Event(2, Message = "Generator {0} {2} phase ran for {3} ticks", Keywords = Keywords.Performance, Level = EventLevel.Informational, Opcode = EventOpcode.Stop, Task = Tasks.GeneratorPhaseRunTime)]
        private void StopGeneratorPhase(string generatorName, string assemblyPath, string phase, long elapsedTicks, string id) =>
            WriteEvent(2, generatorName, assemblyPath, phase, elapsedTicks, id);

        public record struct SourceGeneratorsOperation(string generatorName, string assemblyPath, string phase, string id, long startTicks);
    }
}
