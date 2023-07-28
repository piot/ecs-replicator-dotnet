/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Blitser;
using Piot.Clog;
using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.Exceptions;
using Piot.EcsReplicator.In.Serialize;
using Piot.EcsReplicator.In.Serialize.ReceiveStatus;
using Piot.EcsReplicator.In.Serialize.SnapshotPack;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Pack;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.In
{
    public class SnapshotReceiver : ISnapshotReceiver
    {
        readonly IDataReceiver clientWorld;
        readonly IEventReceiver eventReceiver;

        readonly Dictionary<uint, EntityId> LocalPlayerInputs = new();
        EventSequenceId expectedEventSequenceId;
        byte ignoredSnapshotCountAfterLastReceivedTickId;
        TickId lastAppliedTickId;
        TickId lastReceivedTickId;

        readonly ILog log;

        public SnapshotReceiver(IDataReceiver clientWorld, IEventReceiver eventReceiver, ILog log)
        {
            this.clientWorld = clientWorld;
            this.eventReceiver = eventReceiver;
            this.log = log;
        }

        public SnapshotPack Receive(ReadOnlySpan<byte> completePayload)
        {
            var deltaSnapshotPack =
                SnapshotPackReader.Read(completePayload);

            if (!deltaSnapshotPack.TickIdRange.CanBeFollowing(lastReceivedTickId))
            {
                if (deltaSnapshotPack.TickIdRange.Last < lastReceivedTickId)
                {
                    throw new DeserializeException($"out of order received snapshots {deltaSnapshotPack.TickIdRange} and last received {lastReceivedTickId}");
                }

                ignoredSnapshotCountAfterLastReceivedTickId = (byte)(deltaSnapshotPack.TickIdRange.Last.tickId - lastReceivedTickId.tickId);
                throw new DeserializeException($"can not be following {deltaSnapshotPack.TickIdRange}, {lastReceivedTickId}");
            }

            lastReceivedTickId = deltaSnapshotPack.TickIdRange.Last;
            ignoredSnapshotCountAfterLastReceivedTickId = 0;

            return deltaSnapshotPack;
        }

        public void WriteStatusHeader(IOctetWriter octetWriter)
        {
            SnapshotReceiveStatusWriter.Write(octetWriter, lastReceivedTickId, ignoredSnapshotCountAfterLastReceivedTickId);
        }

        public void Process(SnapshotPack snapshotPack)
        {
            if (!snapshotPack.TickIdRange.CanBeFollowing(lastAppliedTickId))
            {
                throw new("can not receive deltaSnapshotPack ");
            }

            // TODO: Serialize exact bit count
            var bitSnapshotReader =
                new BitReader(snapshotPack.payload.Span, snapshotPack.payload.Length * 8);

            try
            {
                var tickIdRange = snapshotPack.TickIdRange;
                var isMergedAndOverlapping = tickIdRange.IsOverlappingAndMerged(lastAppliedTickId);
                log.Debug("Playback {Snapshot}", snapshotPack);
                expectedEventSequenceId = ApplyDeltaSnapshotToWorld.Apply(bitSnapshotReader, snapshotPack.SnapshotType, clientWorld,
                    eventReceiver, expectedEventSequenceId,
                    isMergedAndOverlapping, log);

                lastAppliedTickId = tickIdRange.Last;
                ReadEntityIdAssignmentsForLocalPlayers(bitSnapshotReader);
            }
            catch (DeserializeException e)
            {
                log.Notice(e.Message);
                throw;
            }
        }

        void ReadEntityIdAssignmentsForLocalPlayers(IBitReader snapshotReader)
        {
            var localPlayerInformationCount = snapshotReader.ReadBits(4);

            for (var i = 0; i < localPlayerInformationCount; ++i)
            {
                var localPlayerIndex = LocalPlayerIndexReader.Read(snapshotReader);
                EntityIdReader.Read(snapshotReader, out var newTargetEntityId);

                var wasFound = LocalPlayerInputs.TryGetValue(localPlayerIndex.Value, out var assignedEntityId);
                if (!wasFound)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, newTargetEntityId);

                    //var createdPredictor = notifyPredictor.CreateAvatarPredictor(localPlayerIndex, targetEntityId);

                    LocalPlayerInputs[localPlayerIndex.Value] = assignedEntityId;
                }
                else
                {
                    if (assignedEntityId.Value == newTargetEntityId.Value)
                    {
                        continue;
                    }

                    log.Debug("switched avatar for {LocalPlayer} {EntityId}", localPlayerIndex, newTargetEntityId);
                    LocalPlayerInputs[localPlayerIndex.Value] = newTargetEntityId;
                }
            }
        }
    }
}