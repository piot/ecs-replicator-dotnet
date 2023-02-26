/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿namespace Piot.EcsReplicator.Out.Syncer
{
    public readonly struct ConnectionId
    {
        public const ushort ReservedForLocalIdValue = ushort.MaxValue;
        public const ushort NoChannelIdValue = 0;
        public static ConnectionId NoEndpoint = new(NoChannelIdValue);

        public ConnectionId(ushort channel)
        {
            Value = channel;
        }

        public ushort Value { get; }

        public override string ToString()
        {
            return $"[ConnectionId {Value}]";
        }
    }
}