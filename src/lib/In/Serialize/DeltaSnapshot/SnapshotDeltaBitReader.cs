/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.In.Serialize.Event;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.DeltaSnapshot
{
    public static class SnapshotDeltaBitReader
    {
        /// <summary>
        ///     Reading a snapshot delta pack and returning the created, deleted and updated entities.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entityContainer"></param>
        /// <returns></returns>
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator,
            IEventReceiver eventProcessor, EventSequenceId nextExpectedSequenceId,
            bool isOverlappingMergedSnapshot, ILog log, bool useEvents = true)
        {
            SnapshotDeltaEntityBitReader.ReadAndApply(reader, entityGhostContainerWithCreator,
                isOverlappingMergedSnapshot, log);

            return useEvents
                ? EventBitReader.ReadAndApply(reader, eventProcessor, nextExpectedSequenceId)
                : EventBitReader.Skip(reader, eventProcessor, nextExpectedSequenceId);
        }
    }
}