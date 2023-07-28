/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.EcsReplicator.Types;

namespace Piot.EcsReplicator.Out.EcsInterfaces
{
    public readonly struct ComponentChangedFieldMask
    {
        public ComponentChangedFieldMask(ulong mask)
        {
            this.mask = mask;
        }

        public readonly ulong mask;

        public override string ToString()
        {
            return $"[FieldChangeMask {mask:x4}]";
        }
    }

    public class EntityChangesForOneEntity
    {
        public readonly Dictionary<ComponentTypeId, ComponentChangedFieldMask> componentChanges = new();
        public EntityId entityId;

        public EntityChangesForOneEntity(EntityId entityId)
        {
            this.entityId = entityId;
        }

        public void Add(ComponentTypeId componentTypeId, ComponentChangedFieldMask fieldMasks)
        {
            componentChanges.Add(componentTypeId, fieldMasks);
        }

        public override string ToString()
        {
            var s = entityId.ToString();
            return componentChanges.Values.Aggregate(s, (current, x) => current + x);
        }
    }

    public class AllEntitiesChangesThisTick
    {
        public readonly Dictionary<uint, EntityChangesForOneEntity> EntitiesComponentChanges = new();

        public override string ToString()
        {
            return EntitiesComponentChanges.Values.Aggregate("", (current, oneEntityChanges) => current + oneEntityChanges);
        }
    }

}
