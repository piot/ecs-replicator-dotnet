/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.EcsReplicator.In.Serialize
{
    public static class LocalPlayerIndexReader
    {
        public static LocalPlayerIndex Read(IBitReader reader)
        {
            return new((byte)reader.ReadBits(4));
        }
    }
}