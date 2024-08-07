﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit
{
    public abstract class ILGenerator
    {
        protected ILGenerator()
        {
        }

        /// <summary>
        /// Creates a <see cref="Label"/> with the given id.
        /// </summary>
        /// <param name="id">The unique id for the label.</param>
        /// <returns>The <see cref="Label"/> created.</returns>
        protected static Label CreateLabel(int id) => new Label(id);

        #region Public Members

        #region Emit
        public abstract void Emit(OpCode opcode);

        public abstract void Emit(OpCode opcode, byte arg);

        public abstract void Emit(OpCode opcode, short arg);

        public abstract void Emit(OpCode opcode, long arg);

        public abstract void Emit(OpCode opcode, float arg);

        public abstract void Emit(OpCode opcode, double arg);

        public abstract void Emit(OpCode opcode, int arg);

        public abstract void Emit(OpCode opcode, MethodInfo meth);

        public abstract void EmitCalli(OpCode opcode, CallingConventions callingConvention,
            Type? returnType, Type[]? parameterTypes, Type[]? optionalParameterTypes);

        public abstract void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type? returnType, Type[]? parameterTypes);

        public abstract void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[]? optionalParameterTypes);

        public abstract void Emit(OpCode opcode, SignatureHelper signature);

        public abstract void Emit(OpCode opcode, ConstructorInfo con);

        public abstract void Emit(OpCode opcode, Type cls);

        public abstract void Emit(OpCode opcode, Label label);

        public abstract void Emit(OpCode opcode, Label[] labels);

        public abstract void Emit(OpCode opcode, FieldInfo field);

        public abstract void Emit(OpCode opcode, string str);

        public abstract void Emit(OpCode opcode, LocalBuilder local);
        #endregion

        #region Exceptions
        public abstract Label BeginExceptionBlock();

        public abstract void EndExceptionBlock();

        public abstract void BeginExceptFilterBlock();

        public abstract void BeginCatchBlock(Type? exceptionType);

        public abstract void BeginFaultBlock();

        public abstract void BeginFinallyBlock();

        #endregion

        #region Labels
        public abstract Label DefineLabel();

        public abstract void MarkLabel(Label loc);

        #endregion

        #region IL Macros
        public virtual void ThrowException([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type excType)
        {
            // Emits the il to throw an exception
            ArgumentNullException.ThrowIfNull(excType);

            if (!excType.IsSubclassOf(typeof(Exception)) && excType != typeof(Exception))
            {
                throw new ArgumentException(SR.Argument_NotExceptionType, nameof(excType));
            }
            ConstructorInfo? con = excType.GetConstructor(Type.EmptyTypes);
            if (con == null)
            {
                throw new ArgumentException(SR.Arg_NoDefCTorWithoutTypeName, nameof(excType));
            }
            Emit(OpCodes.Newobj, con);
            Emit(OpCodes.Throw);
        }

        private const string ConsoleTypeFullName = "System.Console, System.Console";
        private static readonly Type[] s_parameterTypes = [typeof(string)];

        public virtual void EmitWriteLine(string value)
        {
            // Emits the IL to call Console.WriteLine with a string.
            Emit(OpCodes.Ldstr, value);
            Type consoleType = Type.GetType(ConsoleTypeFullName, throwOnError: true)!;
            MethodInfo mi = consoleType.GetMethod("WriteLine", s_parameterTypes)!;
            Emit(OpCodes.Call, mi);
        }

        public virtual void EmitWriteLine(LocalBuilder localBuilder)
        {
            // Emits the IL necessary to call WriteLine with lcl.  It is
            // an error to call EmitWriteLine with a lcl which is not of
            // one of the types for which Console.WriteLine implements overloads. (e.g.
            // we do *not* call ToString on the locals.

            Type consoleType = Type.GetType(ConsoleTypeFullName, throwOnError: true)!;
            MethodInfo prop = consoleType.GetMethod("get_Out")!;
            Emit(OpCodes.Call, prop);
            Emit(OpCodes.Ldloc, localBuilder);
            Type[] parameterTypes = new Type[1];
            Type cls = localBuilder.LocalType;
            if (cls is TypeBuilder || cls is EnumBuilder)
            {
                throw new ArgumentException(SR.NotSupported_OutputStreamUsingTypeBuilder);
            }
            parameterTypes[0] = cls;
            MethodInfo? mi = typeof(IO.TextWriter).GetMethod("WriteLine", parameterTypes);
            if (mi == null)
            {
                throw new ArgumentException(SR.Argument_EmitWriteLineType, nameof(localBuilder));
            }

            Emit(OpCodes.Callvirt, mi);
        }

        public virtual void EmitWriteLine(FieldInfo fld)
        {
            ArgumentNullException.ThrowIfNull(fld);

            // Emits the IL necessary to call WriteLine with fld.  It is
            // an error to call EmitWriteLine with a fld which is not of
            // one of the types for which Console.WriteLine implements overloads. (e.g.
            // we do *not* call ToString on the fields.
            Type consoleType = Type.GetType(ConsoleTypeFullName, throwOnError: true)!;
            MethodInfo prop = consoleType.GetMethod("get_Out")!;
            Emit(OpCodes.Call, prop);

            if ((fld.Attributes & FieldAttributes.Static) != 0)
            {
                Emit(OpCodes.Ldsfld, fld);
            }
            else
            {
                Emit(OpCodes.Ldarg_0); // Load the this ref.
                Emit(OpCodes.Ldfld, fld);
            }
            Type[] parameterTypes = new Type[1];
            Type cls = fld.FieldType;
            if (cls is TypeBuilder || cls is EnumBuilder)
            {
                throw new NotSupportedException(SR.NotSupported_OutputStreamUsingTypeBuilder);
            }
            parameterTypes[0] = cls;
            MethodInfo? mi = typeof(IO.TextWriter).GetMethod("WriteLine", parameterTypes);
            if (mi == null)
            {
                throw new ArgumentException(SR.Argument_EmitWriteLineType, nameof(fld));
            }

            Emit(OpCodes.Callvirt, mi);
        }

        #endregion

        #region Debug API
        public virtual LocalBuilder DeclareLocal(Type localType)
        {
            return DeclareLocal(localType, false);
        }

        public abstract LocalBuilder DeclareLocal(Type localType, bool pinned);

        public abstract void UsingNamespace(string usingNamespace);

        public abstract void BeginScope();

        public abstract void EndScope();

        public abstract int ILOffset { get; }

        [CLSCompliant(false)]
        public void Emit(OpCode opcode, sbyte arg) => Emit(opcode, (byte)arg);

        /// <summary>
        /// Marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="document">The document for which the sequence point is being defined.</param>
        /// <param name="startLine">The line where the sequence point begins.</param>
        /// <param name="startColumn">The column in the line where the sequence point begins.</param>
        /// <param name="endLine">The line where the sequence point ends.</param>
        /// <param name="endColumn">The column in the line where the sequence point ends.</param>
        /// <exception cref="ArgumentNullException"><paramref name="document"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="document"/> is not valid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startLine"/> is not within range [0, 0x20000000) or
        /// <paramref name="endLine"/> is not within range [0, 0x20000000) or lower than <paramref name="startLine"/> or
        /// <paramref name="startColumn"/> is not within range [0, 0x10000) or
        /// <paramref name="endLine"/> is not within range [0, 0x10000) or
        /// <paramref name="startLine"/> equal to <paramref name="endLine"/> and it is not hidden sequence point and <paramref name="endLine"/> lower than or equal to <paramref name="startLine"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">Emitting debug info is not supported.</exception>"
        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            ArgumentNullException.ThrowIfNull(document);

            if (startLine < 0 || startLine >= 0x20000000)
            {
                throw new ArgumentOutOfRangeException(nameof(startLine));
            }

            if (endLine < 0 || endLine >= 0x20000000 || startLine > endLine)
            {
                throw new ArgumentOutOfRangeException(nameof(endLine));
            }

            if (startColumn < 0 || startColumn >= 0x10000)
            {
                throw new ArgumentOutOfRangeException(nameof(startColumn));
            }

            if (endColumn < 0 || endColumn >= 0x10000 ||
                (startLine == endLine && startLine != 0xfeefee && startColumn >= endColumn))
            {
                throw new ArgumentOutOfRangeException(nameof(endColumn));
            }

            MarkSequencePointCore(document, startLine, startColumn, endLine, endColumn);
        }

        /// <summary>
        /// When overridden in a derived class, marks a sequence point in the Microsoft intermediate language (MSIL) stream.
        /// </summary>
        /// <param name="document">The document for which the sequence point is being defined.</param>
        /// <param name="startLine">The line where the sequence point begins.</param>
        /// <param name="startColumn">The column in the line where the sequence point begins.</param>
        /// <param name="endLine">The line where the sequence point ends.</param>
        /// <param name="endColumn">The column in the line where the sequence point ends.</param>
        /// <exception cref="ArgumentException"><paramref name="document"/> is not valid.</exception>
        /// <exception cref="NotSupportedException">Emitting debug info is not supported.</exception>"
        /// <remarks>The parameters validated in the caller: <see cref="MarkSequencePoint"/>.</remarks>
        protected virtual void MarkSequencePointCore(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn) =>
            throw new NotSupportedException(SR.NotSupported_EmitDebugInfo);

        #endregion

        #endregion
    }
}
