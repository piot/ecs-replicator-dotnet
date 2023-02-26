/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.EcsReplicator.Types;
using Piot.Flood;

namespace Piot.EcsReplicator.Out.Serialize.Types
{
    public static class EntityIdWriter
    {
        public static void Write(IBitWriter writer, EntityId id)
        {
            BitMarker.WriteMarker(writer, 0xaf);
            writer.WriteBits(id.Value, 16);
        }
    }

    public static class ComponentTypeIdWriter
    {
        public static void Write(IBitWriter writer, ComponentTypeId componentTypeId)
        {
            writer.WriteBits(componentTypeId.id, 8);
        }
    }
}