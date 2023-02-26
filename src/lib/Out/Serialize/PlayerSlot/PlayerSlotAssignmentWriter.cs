/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/


using System.Collections.Generic;
using Piot.EcsReplicator.Out.Serialize;
using Piot.EcsReplicator.Out.Serialize.Types;
using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Syncer
{
    public class LocalPlayerAssignments
    {
        public EntityId entityToControl;
        public EntityId playerSlotEntity;
        public bool shouldPredict;

        public LocalPlayerAssignments()
        {

        }

        public LocalPlayerAssignments(EntityId playerSlotEntity)
        {
            this.playerSlotEntity = playerSlotEntity;
        }
    }

    public static class PlayerSlotAssignmentWriter
    {
        public static void Write(Dictionary<LocalPlayerIndex, LocalPlayerAssignments> playerSlotAssignmentForLocalPlayers, IBitWriter writer)
        {
            writer.WriteBits((byte)playerSlotAssignmentForLocalPlayers.Keys
                .Count, 3);

            foreach (var localPlayerAssignedPredictedEntity in playerSlotAssignmentForLocalPlayers)
            {
                LocalPlayerIndexWriter.Write(localPlayerAssignedPredictedEntity.Key, writer);

                var predictionEntity = localPlayerAssignedPredictedEntity.Value;
                EntityIdWriter.Write(writer, predictionEntity.playerSlotEntity);
                EntityIdWriter.Write(writer, predictionEntity.entityToControl);
                writer.WriteBits(predictionEntity.shouldPredict ? 1U : 0U, 1);
            }
        }
    }
}