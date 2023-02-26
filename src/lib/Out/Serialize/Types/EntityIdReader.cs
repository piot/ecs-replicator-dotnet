/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.Types
{
    public static class EntityIdReader
    {
        public static void Read(IBitReader reader, out EntityId id)
        {
            BitMarker.AssertMarker(reader, 0xaf);
            id = new((ushort)reader.ReadBits(16));
        }
    }

    public static class ComponentTypeIdReader
    {
        public static ComponentTypeId Read(IBitReader reader)
        {
            return new((ushort)reader.ReadBits(8));
        }
    }
}