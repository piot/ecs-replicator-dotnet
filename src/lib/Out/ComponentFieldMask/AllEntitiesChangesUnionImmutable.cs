/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.EcsReplicator.Out.ComponentFieldMask
{
    public readonly struct AllEntitiesChangesUnionImmutable
    {
        public readonly Dictionary<uint, ComponentChangesForOneEntityImmutable> EntitiesComponentChanges;
        readonly TickIdRange TickIdRange;

        public AllEntitiesChangesUnionImmutable(TickIdRange tickIdRange, Dictionary<uint, ComponentChangesForOneEntityImmutable> immutables)
        {
            TickIdRange = tickIdRange;
            EntitiesComponentChanges = immutables;
        }

        public override string ToString()
        {
            var s = "";

            foreach (var change in EntitiesComponentChanges.Values)
            {
                s += $" {change}\n";
            }

            return $"[AllChanges range:{TickIdRange} count:{EntitiesComponentChanges.Count} changes:{s}]";
        }
    }
}