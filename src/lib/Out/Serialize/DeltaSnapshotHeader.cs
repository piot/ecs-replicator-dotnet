/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Out.Serialize.Snapshot;
using Piot.EcsReplicator.Out.Serialize.Tick;
using Piot.EcsReplicator.Pack;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize
{
    public static class DeltaSnapshotHeader
    {
        public static void Write(SnapshotPack snapshotIncludingPredictionAssignmentPack,
            IOctetWriter writer, IOctetWriterWithResult subWriter)
        {
#if DEBUG
            writer.WriteUInt8(Constants.SnapshotPackSync);
#endif
            var snapshotMode = (byte)0;

//            snapshotMode |= (byte)((compressorIndex.Index & 0x03) << 2);

            snapshotMode |=
                (byte)(SnapshotTypeWriter.ToSnapshotMode(snapshotIncludingPredictionAssignmentPack.SnapshotType) << 4);

            writer.WriteUInt8(snapshotMode);

            TickIdRangeWriter.Write(writer, snapshotIncludingPredictionAssignmentPack.TickIdRange);
            subWriter.WriteUInt16((ushort)snapshotIncludingPredictionAssignmentPack.payload.Length);
            subWriter.WriteOctets(snapshotIncludingPredictionAssignmentPack.payload.Span);

            writer.WriteOctets(subWriter.Octets);
        }
    }
}