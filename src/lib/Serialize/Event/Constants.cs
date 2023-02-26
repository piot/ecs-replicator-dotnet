/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.EcsReplicator.Event.Serialization
{
    public static class EventConstants
    {
        public const byte ShortLivedEventsStartSync = 0x88;
        public const int MaxEventQueueOctetSize = 1024;
    }
}