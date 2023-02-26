/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Event;
using Piot.EcsReplicator.Event.Serialization;
using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize.Event
{
    public static class EventStreamHeaderReader
    {
        public static (byte, EventSequenceId) Read(IBitReader reader)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, EventConstants.ShortLivedEventsStartSync);
#endif
            var count = (byte)reader.ReadBits(6);

            if (count == 0)
            {
                return (count, new(0));
            }

            var sequenceId = EventSequenceIdReader.Read(reader);
            return (count, sequenceId);
        }
    }
}