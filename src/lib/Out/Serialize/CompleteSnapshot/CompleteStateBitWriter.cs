/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.Out.EcsInterfaces;
using Piot.EcsReplicator.Out.Event;
using Piot.EcsReplicator.Out.Serialize.Event;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.CompleteSnapshot
{
    public static class CompleteStateBitWriter
    {
        public static ReadOnlySpan<byte> CaptureCompleteSnapshotPack(IDataSender world, uint[] clientSidePredictedEntities,
            EventSequenceId expectedShortLivedEventSequenceId, IBitWriterWithResult bitWriter, ILog log)
        {
#if DEBUG
            BitMarker.WriteMarker(bitWriter, SnapshotConstants.CompleteSnapshotStartMarker);
#endif

            var entityIds = world.AllEntities();
            if (entityIds.Length == 0)
            {
                log.Debug("Strange, entities to write is zero");
            }

            foreach (var entityIdToSerialize in entityIds)
            {
                var hasWrittenEntityId = false;
                foreach (var componentTypeId in DataInfo.ghostComponentTypeIds!)
                {
                    if (!world.HasComponentTypeId(entityIdToSerialize, (ushort)componentTypeId))
                    {
                        log.Debug("{EntityId} did not contain {ComponentTypeId} so skipping it", entityIdToSerialize, componentTypeId);
                        continue;
                    }

                    if (!hasWrittenEntityId)
                    {
                        log.Debug("Writing {EntityId}", entityIdToSerialize);
                        EntityIdWriter.Write(bitWriter, new((ushort)entityIdToSerialize));
                        hasWrittenEntityId = true;
                    }


                    log.Debug("Writing {ComponentTypeId}", componentTypeId);
                    ComponentTypeIdWriter.Write(bitWriter, new((ushort)componentTypeId));

                    world.WriteFull(bitWriter, entityIdToSerialize, (ushort)componentTypeId);
                }

                if (hasWrittenEntityId)
                {
                    ComponentTypeIdWriter.Write(bitWriter, ComponentTypeId.None);
                }
            }

            EntityIdWriter.Write(bitWriter, EntityId.None);

            EventSequenceIdWriter.Write(bitWriter, expectedShortLivedEventSequenceId);

            EventsWriter.Write(Array.Empty<EventStreamPackItem>(), bitWriter);

            return bitWriter.Close(out _);
        }
    }
}