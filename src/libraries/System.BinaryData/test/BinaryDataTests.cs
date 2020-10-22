// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Xunit;

namespace System.BinaryDataTests
{
    public class BinaryDataTests
    {
        [Fact]
        public void CanCreateBinaryData()
        {
            BinaryData result = BinaryData.FromBytes(new byte[0]);
            // bogus placeholder test
            Assert.Equal(default(BinaryData), result);
        }
    }
}
