/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.DeltaSnapshot
{
    public static class SnapshotDeltaEntityBitReader
    {
        public static void ReadAndApply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator, bool isOverlappingMergedSnapshot, ILog log)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.SnapshotDeltaSync);
#endif
            while (true)
            {
                var entityId = new EntityId();
                EntityIdReader.Read(reader, out entityId);

                if (entityId.Value == EntityId.NoneValue)
                {
                    log.Debug("Detected end of delta update");
                    break;
                }

                log.Debug("Select entity {EntityId} for changes", entityId);

                while (true)
                {
                    var componentTypeId = ComponentTypeIdReader.Read(reader);
                    if (componentTypeId.id == ComponentTypeId.NoneValue)
                    {
                        log.Debug("Detected end of {EntityId} changes", entityId);

                        break;
                    }

                    var isAlive = reader.ReadBits(1) != 0;
                    if (isAlive)
                    {
                        log.Debug("Receive Update {EntityId} {ComponentTypeId}", entityId, componentTypeId);
                        DataStreamReceiver.ReceiveUpdate(reader, entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                    }
                    else
                    {
                        log.Debug("Receive Destroy {EntityId} {ComponentTypeId}", entityId, componentTypeId);
                        DataStreamReceiver.ReceiveDestroy(entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                    }
                }
            }
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.SnapshotDeltaEventSync);
#endif
        }
    }
}