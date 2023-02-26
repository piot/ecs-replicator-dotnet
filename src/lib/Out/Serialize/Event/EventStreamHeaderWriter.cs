/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Event.Serialization;
using Piot.EcsReplicator.Out.Event;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.Event
{
    public static class EventStreamHeaderWriter
    {
        public static void Write(IBitWriter writer, EventStreamPackItem[] events)
        {
#if DEBUG
            BitMarker.WriteMarker(writer, EventConstants.ShortLivedEventsStartSync);
#endif
            writer.WriteBits((byte)events.Length, 6);

            if (events.Length > 0)
            {
                EventSequenceIdWriter.Write(writer, events[0].eventSequenceId);
            }
        }
    }
}