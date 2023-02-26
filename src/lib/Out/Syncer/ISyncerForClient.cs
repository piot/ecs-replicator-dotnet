/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿using System;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Syncer
{
    public interface ISyncerForClient
    {
        public void ReceiveStatusFromReceiver(ReadOnlySpan<byte> octets);
        public void ReceiveStatusFromReceiver(IOctetReader reader);
        public void ReceiveStatusFromReceiver(TickId tickId, uint droppedCount);
    }
}