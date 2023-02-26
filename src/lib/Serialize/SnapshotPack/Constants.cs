/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.EcsReplicator
{
    public static class Constants
    {
        public const byte SnapshotReceiveStatusSync = 0x18;
        public const byte SnapshotPackSync = 0xba;
        public const byte SnapshotDeltaSync = 0xbb;
        public const byte SnapshotDeltaEventSync = 0xbc;
        public const uint MaxSnapshotOctetSize = 64 * 1200;
    }
}