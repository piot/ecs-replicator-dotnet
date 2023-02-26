/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.EcsReplicator.Types;

namespace Piot.EcsReplicator.Out.ComponentFieldMask
{
    public struct ComponentChangesForOneEntityImmutable
    {
        public EntityId entityId;
        public readonly Dictionary<ushort, ulong> componentChangesMasks;
        ushort[] deletedComponentTypeIds;

        public ComponentChangesForOneEntityImmutable(EntityId entityId, Dictionary<ushort, ulong> componentChanges)
        {
            componentChangesMasks = componentChanges;
            var deleted = new List<ushort>();
            foreach (var componentChangePair in componentChanges)
            {
                if ((componentChangePair.Value & ChangedFieldsMask.DeletedMaskBit) == ChangedFieldsMask.DeletedMaskBit)
                {
                    deleted.Add(componentChangePair.Key);
                }
            }

            deletedComponentTypeIds = deleted.ToArray();

            this.entityId = entityId;
        }

        public override string ToString()
        {
            var s = $"[changeForEntityImm {entityId} ";

            foreach (var componentFieldMask in componentChangesMasks)
            {
                s += $"{componentFieldMask.Key}:{componentFieldMask.Value:X016} ";
            }

            s += "]";

            return s;
        }
    }
}