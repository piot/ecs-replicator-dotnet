/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.CompleteSnapshot
{
    public static class CompleteStateEntityBitReader
    {
        public static void Apply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator, ILog log)
        {
            while (true)
            {
                var entityId = new EntityId();
                EntityIdReader.Read(reader, out entityId);

                if (entityId.Value == EntityId.NoneValue)
                {
                    log.Debug("Complete State Done");

                    break;
                }

                log.Debug("Complete State. Select {EntityId} for changes", entityId.Value);

                while (true)
                {
                    var componentTypeId = ComponentTypeIdReader.Read(reader);
                    if (componentTypeId.id == ComponentTypeId.NoneValue)
                    {
                        log.Debug("Complete State. No more changes for {EntityId}", entityId.Value);
                        break;
                    }

                    log.Debug("Complete State. Create/Update {ComponentType} on {EntityId}", componentTypeId.id, entityId.Value);
                    DataStreamReceiver.ReceiveNew(reader, entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                }
            }
        }
    }
}