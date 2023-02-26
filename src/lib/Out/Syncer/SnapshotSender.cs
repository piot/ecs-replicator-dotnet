/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.EcsReplicator.Out.ComponentFieldMask;
using Piot.EcsReplicator.Out.EcsInterfaces;
using Piot.EcsReplicator.Out.Event;
using Piot.EcsReplicator.Out.Serialize;
using Piot.EcsReplicator.Out.Serialize.CompleteSnapshot;
using Piot.EcsReplicator.Out.Serialize.DeltaSnapshot;
using Piot.EcsReplicator.Pack;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Syncer
{
    public sealed class SnapshotSender : ISnapshotSender
    {
        readonly BitWriter cachedBitWriterForSnapshots = new(Constants.MaxSnapshotOctetSize);
        readonly OctetWriter cachedCompleteCompressedDeltaSnapshotPackWriter = new(12 * 1024);
        readonly OctetWriter cachedCompressionWriter = new(12 * 1024);
        readonly BitWriter cachedPlayerSlotAssignmentWriter = new(10 * 1024);
        readonly ILog log;
        readonly List<SnapshotSyncerForClientConnection> syncClients = new();
        TickId allClientsAreWaitingForAtLeastTickId;
        TickId authoritativeTickId;
        readonly EventStreamPackQueue eventStream;
        readonly IDataSender world;
        readonly IEntityContainerWithDetectChanges worldWithChanges;

        public SnapshotSender(IDataSender world, IEntityContainerWithDetectChanges worldWithChanges, EventStreamPackQueue eventStream, ILog log)
        {
            this.world = world;
            this.worldWithChanges = worldWithChanges;
            this.eventStream = eventStream;
            this.log = log;
        }

        public AllEntitiesChangesEachTickHistory History { get; } = new();

        public IEnumerable<SnapshotSyncerForClientConnection> ClientSyncers => syncClients;

        public void CreateSnapshot()
        {
            authoritativeTickId = authoritativeTickId.Next;
            StoreChangesMadeToTheWorld();
        }


        public ReadOnlySpan<byte> SendSnapshotTo(SnapshotSyncerForClientConnection connection)
        {
            var deltaSnapshotPack = CreateConnectionSpecificSnapshot(connection);

            log.DebugLowLevel("Sending assigned physics correction {LocalPlayerCount}",
                connection.AssignedPredictedEntityForLocalPlayers.Keys.Count);

            cachedPlayerSlotAssignmentWriter.Reset();
            PlayerSlotAssignmentWriter.Write(connection.AssignedPredictedEntityForLocalPlayers, cachedPlayerSlotAssignmentWriter);

            cachedCompleteCompressedDeltaSnapshotPackWriter.Reset();
            cachedCompressionWriter.Reset();
            DeltaSnapshotHeader.Write(deltaSnapshotPack, cachedCompleteCompressedDeltaSnapshotPackWriter, cachedCompressionWriter);

            log.Debug("wrote {Snapshot}", deltaSnapshotPack);

            return cachedCompleteCompressedDeltaSnapshotPackWriter.Octets;
        }
        
        public ReadOnlySpan<byte> SendSnapshotTo(ISyncerForClient connection)
        {
            return SendSnapshotTo((SnapshotSyncerForClientConnection) connection);
        }

        void HandleNotifyExpectedTickId(TickId _)
        {
            TickId lowestTickId = new(0);
            var hasBeenSet = false;
            foreach (var syncClient in syncClients)
            {
                if (syncClient.RemoteIsExpectingTickId <= allClientsAreWaitingForAtLeastTickId)
                {
                    // We can give up early, this client is still waiting for the same tick id
                    return;
                }

                if (!hasBeenSet || syncClient.RemoteIsExpectingTickId < lowestTickId)
                {
                    lowestTickId = syncClient.RemoteIsExpectingTickId;
                    hasBeenSet = true;
                }
            }

            if (!hasBeenSet || lowestTickId < allClientsAreWaitingForAtLeastTickId)
            {
                return;
            }

            allClientsAreWaitingForAtLeastTickId = lowestTickId;
            log.Debug("no connection is needing history for older than {TickId}, so deleting the old ones", allClientsAreWaitingForAtLeastTickId);
            History.DiscardUpTo(allClientsAreWaitingForAtLeastTickId);
        }

        public SnapshotSyncerForClientConnection Create(ConnectionId id)
        {
            var client = new SnapshotSyncerForClientConnection(id, HandleNotifyExpectedTickId, log.SubLog($"Syncer{id}"));

            syncClients.Add(client);

            return client;
        }
        
        public ISyncerForClient CreateSyncer(ConnectionId id)
        {
            return Create(id);
        }


        void StoreChangesMadeToTheWorld()
        {
            var changes = worldWithChanges.EntitiesThatHasChanged(log);
            var extraChanges = new AllEntitiesChangesThisSnapshot
            {
                EcsChanges = changes, TickId = authoritativeTickId
            };

            log.Debug("Store World changes {Changes}", extraChanges);
            History.Enqueue(extraChanges);
        }

        SnapshotPack CreateConnectionSpecificSnapshot(SnapshotSyncerForClientConnection connection)
        {
            if (!connection.HasReceivedInitialState)
            {
                cachedBitWriterForSnapshots.Reset();
                // Send Complete State
                var octets = CompleteStateBitWriter.CaptureCompleteSnapshotPack(world, connection.AssignedPredictedEntityIdValuesForLocalPlayers, eventStream.NextSequenceId, cachedBitWriterForSnapshots, log);
                return new(new(new(0), authoritativeTickId), octets, SnapshotType.CompleteState);
            }

            // In most cases, just send the delta that happened this tick
            var rangeToSend = TickIdRange.FromTickId(authoritativeTickId);
            if (connection.WantsResend)
            {
                rangeToSend = new(connection.RemoteIsExpectingTickId, authoritativeTickId);
            }

            var combinedMasks = History.Fetch(rangeToSend, log);
            log.Debug("fetch {CombinedMasks}", combinedMasks);

            var eventsForThisRange = eventStream.FetchEventsForRange(rangeToSend);
            if (eventsForThisRange.Length > 0)
            {
                log.DebugLowLevel("fetched event packs for {TickId} to {EndTickId}", eventsForThisRange[0],
                    eventsForThisRange[^1]);
            }

            cachedBitWriterForSnapshots.Reset();

            var clientSidePredictedEntities = connection.AssignedPredictedEntityForLocalPlayers.Values.Where(x => x.shouldPredict).Select(x => (uint)x.entityToControl.Value).ToArray();
            return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(world, eventsForThisRange, clientSidePredictedEntities, combinedMasks,
                rangeToSend, cachedBitWriterForSnapshots, log);
        }
    }
}