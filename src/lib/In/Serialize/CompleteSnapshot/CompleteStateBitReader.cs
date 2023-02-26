/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.In.Serialize.Event;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.CompleteSnapshot
{
    public static class CompleteStateBitReader
    {
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator,
            IEventReceiver eventProcessor, ILog log, bool useEvents = true)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, SnapshotConstants.CompleteSnapshotStartMarker);
#endif
            CompleteStateEntityBitReader.Apply(reader, entityGhostContainerWithCreator, log);

            return useEvents
                ? EventCompleteBitReader.ReadAndApply(eventProcessor, reader)
                : EventCompleteBitReader.Skip(reader);
        }
    }
}