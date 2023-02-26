/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.EcsReplicator.In.Serialize.Tick;
using Piot.EcsReplicator.Out.Serialize;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.SnapshotPack
{
    public static class SnapshotPackReader
    {
        /// <summary>
        ///     ReadAndApply on the client coming from the host.
        ///     In many cases the union only consists of a single delta compressed snapshot.
        ///     But in the case of previously dropped snapshots, host resends snapshots in
        ///     ascending, consecutive order.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Pack.SnapshotPack Read(ReadOnlySpan<byte> datagramOctets)
        {
            var headerSize = 6;
#if DEBUG
            headerSize++;
#endif
            var headerReader = new OctetReader(datagramOctets.Slice(0, headerSize));
#if DEBUG
            if (headerReader.ReadUInt8() != Constants.SnapshotPackSync)
            {
                throw new("out of sync");
            }
#endif
            var snapshotMode = headerReader.ReadUInt8();

            var snapshotTypeValue = (uint)(snapshotMode >> 4 & 0x03);

            var snapshotType = snapshotTypeValue switch
            {
                0x00 => SnapshotType.CompleteState,
                0x01 => SnapshotType.DeltaSnapshot
            };

            var tickIdRange = TickIdRangeReader.Read(headerReader);


            var rest = datagramOctets[headerSize..];

            var reader = new OctetReader(rest);

            var payloadOctetCount = reader.ReadUInt16();
            var payload = reader.ReadOctets(payloadOctetCount);

            return new(tickIdRange, payload, snapshotType);
        }
    }
}