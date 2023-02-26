/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿using System;

namespace Piot.EcsReplicator.Out.Syncer
{
    public interface ISnapshotSender
    {
        void CreateSnapshot();
        public ReadOnlySpan<byte> SendSnapshotTo(SnapshotSyncerForClientConnection connection);
    }
}