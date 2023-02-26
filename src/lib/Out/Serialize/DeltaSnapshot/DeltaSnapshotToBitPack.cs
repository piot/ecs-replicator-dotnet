/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Out.ComponentFieldMask;
using Piot.EcsReplicator.Out.EcsInterfaces;
using Piot.EcsReplicator.Out.Event;
using Piot.EcsReplicator.Out.Serialize.Event;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Pack;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.DeltaSnapshot
{
    public static class DeltaSnapshotToBitPack
    {
        /// <summary>
        ///     Creates a pack from a deltaSnapshotEntityIds delta.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="deltaSnapshotEntityIds"></param>
        /// <returns></returns>
        public static SnapshotPack ToDeltaSnapshotPack(IDataSender dataSender,
            EventStreamPackItem[] shortLivedEvents, uint[] clientSidePredictedEntities,
            AllEntitiesChangesUnionImmutable allChanges, TickIdRange tickIdRange, IBitWriterWithResult writer, ILog log)
        {
#if DEBUG
            BitMarker.WriteMarker(writer, Constants.SnapshotDeltaSync);
#endif

            if (allChanges.EntitiesComponentChanges.Count == 0)
            {
                log.Notice("No components changed this {TickIdRange}", tickIdRange);
            }

            foreach (var changeForOneEntity in allChanges.EntitiesComponentChanges)
            {
                var entityId = changeForOneEntity.Value.entityId;
                var haveWrittenEntityId = false;
                var isClientSidePredicted = clientSidePredictedEntities.Contains(entityId.Value);
                foreach (var componentChange in changeForOneEntity.Value.componentChangesMasks)
                {
                    var componentTypeId = new ComponentTypeId(componentChange.Key);

                    var isInputComponent = DataInfo.inputComponentTypeIds!.Contains(componentTypeId.id);
                    // Input is only coming from Client to Host and never the other way around
                    if (isInputComponent)
                    {
                        continue;
                    }

                    var isLogicComponent = DataInfo.logicComponentTypeIds!.Contains(componentTypeId.id);
                    if (isClientSidePredicted)
                    {
                        if (!isLogicComponent)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (isLogicComponent)
                        {
                            continue;
                        }
                    }

                    if (!haveWrittenEntityId)
                    {
                        EntityIdWriter.Write(writer, entityId);
                        haveWrittenEntityId = true;
                    }

                    ComponentTypeIdWriter.Write(writer, componentTypeId);
                    var changedFieldMask = componentChange.Value;
                    var wasDeleted = changedFieldMask == ChangedFieldsMask.DeletedMaskBit;
                    var foundInfo = DataMetaInfo.GetMeta(componentTypeId.id);
                    if (foundInfo is null)
                    {
                        // throw new($"internal error. can not find meta info for {componentTypeId}");
                    }

                    log.Debug("Writing Component to Client {EntityId} {ComponentTypeId}", entityId.Value, componentTypeId);
                    writer.WriteBits(wasDeleted ? 0U : 1U, 1);
                    if (!wasDeleted)
                    {
                        dataSender.WriteMask(writer, entityId.Value, componentTypeId.id, changedFieldMask);
                    }
                }

                if (haveWrittenEntityId)
                {
                    ComponentTypeIdWriter.Write(writer, ComponentTypeId.None);
                }
            }

            EntityIdWriter.Write(writer, EntityId.None);

#if DEBUG
            BitMarker.WriteMarker(writer, Constants.SnapshotDeltaEventSync);
#endif
            EventsWriter.Write(shortLivedEvents, writer);

            return new(tickIdRange, writer.Close(out _), SnapshotType.DeltaSnapshot);
        }
    }
}