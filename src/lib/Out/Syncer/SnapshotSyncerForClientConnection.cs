/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.EcsReplicator.Out.Serialize.ReceiveStatus;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Syncer
{
    public sealed class SnapshotSyncerForClientConnection : ISyncerForClient
    {
        readonly Action<TickId> OnNotifyExpectingTickId;
        readonly ILog log;

        public SnapshotSyncerForClientConnection(ConnectionId id, Action<TickId> onNotifyExpectingTickId, ILog log)
        {
            OnNotifyExpectingTickId = onNotifyExpectingTickId;
            Endpoint = id;
            this.log = log;
        }

        public bool HasReceivedInitialState => RemoteIsExpectingTickId.tickId != 0;

        public TickId RemoteIsExpectingTickId { private set; get; }

        public Dictionary<LocalPlayerIndex, LocalPlayerAssignments> AssignedPredictedEntityForLocalPlayers { get; } = new();

        public uint[] AssignedPredictedEntityIdValuesForLocalPlayers => AssignedPredictedEntityForLocalPlayers.Select(x => (uint)x.Value.entityToControl.Value).ToArray();

        public bool WantsResend { get; private set; }

        public ConnectionId Endpoint { get; }

        public void ReceiveStatusFromReceiver(ReadOnlySpan<byte> octets)
        {
            ReceiveStatusFromReceiver(new OctetReader(octets));
        }

        public void ReceiveStatusFromReceiver(IOctetReader reader)
        {
            SnapshotReceiveStatusReader.Read(reader, out var expectingTickId, out var droppedFramesAfterThat);
            ReceiveStatusFromReceiver(expectingTickId, droppedFramesAfterThat);
        }

        public void ReceiveStatusFromReceiver(TickId expectingTickId, uint droppedCount)
        {
            log.Debug("Received information that remote is expecting {ExpectingTickId} and has dropped {DroppedCount} after that", expectingTickId, droppedCount);
            if (expectingTickId != RemoteIsExpectingTickId)
            {
                OnNotifyExpectingTickId(expectingTickId);
            }

            RemoteIsExpectingTickId = expectingTickId;
            WantsResend = droppedCount > 0;
        }

        public void SetEntityToControl(LocalPlayerIndex localPlayerIndex, EntityId predictEntityId, bool shouldPredict)
        {
            if (predictEntityId.Value == 0)
            {
                throw new("entity must be set");
            }

            var wasFound = AssignedPredictedEntityForLocalPlayers.TryGetValue(localPlayerIndex, out var existingAssignment);
            if (!wasFound || existingAssignment is null)
            {
                throw new($"can not set an predicted entity for {localPlayerIndex} before player slot entity Id is assigned");
            }

            existingAssignment.entityToControl = predictEntityId;
            existingAssignment.shouldPredict = shouldPredict;
        }

        public void SetAssignedPlayerSlotEntity(LocalPlayerIndex localPlayerIndex, EntityId playerSlotEntity)
        {
            if (playerSlotEntity.Value == 0)
            {
                throw new("entity must be set");
            }

            var wasFound = AssignedPredictedEntityForLocalPlayers.TryGetValue(localPlayerIndex, out var existingAssignment);
            if (!wasFound || existingAssignment is null)
            {
                existingAssignment = new(playerSlotEntity);
                AssignedPredictedEntityForLocalPlayers.Add(localPlayerIndex, existingAssignment);
            }
            else
            {
                existingAssignment.entityToControl = playerSlotEntity;
            }
        }
    }
}