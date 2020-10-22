// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    public class BinaryData
    {
        public BinaryData(byte[] data) { }
        public BinaryData(object jsonSerializable, JsonSerializerOptions options = default, Type? type = null) { }
        public BinaryData(ReadOnlyMemory<byte> data) { }
        public BinaryData(string data) { }
        public static BinaryData FromBytes(ReadOnlyMemory<byte> data) => default(BinaryData);
        public static BinaryData FromBytes(byte[] data) => default(BinaryData);
        public static BinaryData FromObjectAsJson<T>(T jsonSerializable, JsonSerializerOptions options = default, CancellationToken cancellationToken = default) => default(BinaryData);
        public static BinaryData FromStream(Stream stream) => default(BinaryData);
        public static Task<BinaryData> FromStreamAsync(Stream stream, CancellationToken cancellationToken = default) => default(Task<BinaryData>);
        public static BinaryData FromString(string data) => default(BinaryData);
        public static implicit operator ReadOnlyMemory<byte>(BinaryData data) => default(ReadOnlyMemory<byte>);
        public static implicit operator ReadOnlySpan<byte>(BinaryData data) => default(ReadOnlySpan<byte>);
        public ReadOnlyMemory<byte> AsBytes() => default(ReadOnlyMemory<byte>);
        public T ToObjectFromJson<T>(JsonSerializerOptions options = default, CancellationToken cancellationToken = default) => default(T);
        public Stream ToStream() => default(Stream);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj) => default(bool);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => default(int);
        public override string ToString() => default(string);
    }
}