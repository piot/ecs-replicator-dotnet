/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿using System;
using Piot.EcsReplicator.Pack;
using Piot.Flood;

namespace Piot.EcsReplicator.In
{
    public interface ISnapshotReceiver
    {
        public SnapshotPack Receive(ReadOnlySpan<byte> completePayload);
        public void WriteStatusHeader(IOctetWriter octetWriter);
        public void Process(SnapshotPack snapshotPack);
    }
}