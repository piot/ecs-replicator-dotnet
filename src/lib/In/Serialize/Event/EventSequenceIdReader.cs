/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Event;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.Event
{
    public static class EventSequenceIdReader
    {
        public static EventSequenceId Read(IBitReader reader)
        {
            return new((ushort)reader.ReadBits(16));
        }

        public static EventSequenceId Read(IOctetReader reader)
        {
            return new(reader.ReadUInt16());
        }
    }
}