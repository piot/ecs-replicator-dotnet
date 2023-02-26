/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.In.Serialize.Tick;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.ReceiveStatus
{
    public static class SnapshotReceiveStatusReader
    {
        /// <summary>
        ///     Read on host coming from client.
        ///     Client describes the last delta compressed snapshot TickId it has received and how many snapshots it has
        ///     detected as dropped after that.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expectingTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Read(IOctetReader reader, out TickId expectingTickId,
            out byte droppedFramesAfterThat)
        {
#if DEBUG
            OctetMarker.AssertMarker(reader, Constants.SnapshotReceiveStatusSync);
#endif
            expectingTickId = TickIdReader.Read(reader);
            droppedFramesAfterThat = reader.ReadUInt8();
        }
    }
}