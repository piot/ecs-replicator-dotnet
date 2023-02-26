/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.In.Serialize.CompleteSnapshot;
using Piot.EcsReplicator.In.Serialize.DeltaSnapshot;
using Piot.EcsReplicator.Out.Serialize;
using Piot.Flood;

namespace Piot.EcsReplicator.In
{
    public static class ApplyDeltaSnapshotToWorld
    {
        public static EventSequenceId Apply(IBitReader bitSnapshotReader, SnapshotType snapshotType, IDataReceiver world,
            IEventReceiver eventProcessor, EventSequenceId expectedEventSequenceId,
            bool isOverlappingMergedSnapshot, ILog log)
        {
            if (snapshotType == SnapshotType.DeltaSnapshot)
            {
                expectedEventSequenceId = SnapshotDeltaBitReader.ReadAndApply(bitSnapshotReader, world,
                    eventProcessor,
                    expectedEventSequenceId, isOverlappingMergedSnapshot, log);
            }
            else
            {
                expectedEventSequenceId =
                    CompleteStateBitReader.ReadAndApply(bitSnapshotReader, world, eventProcessor,
                        log);
            }

            return expectedEventSequenceId;
        }
    }
}