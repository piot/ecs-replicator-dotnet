/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.EcsReplicator.Types;

namespace Piot.EcsReplicator.Out.ComponentFieldMask
{

    public interface IEntityContainerWithDetectChanges
    {
        public AllEntitiesChangesThisTick EntitiesThatHasChanged(ILog log);
    }


    /// <summary>
    ///     Holds a bit mask with a bit for each field in a Component.
    /// </summary>
    public readonly struct ChangedFieldsMask
    {
        public const ulong DeletedMaskBit = 0x8000000000000000;

    }

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

    public struct AllEntitiesChangesThisSnapshot
    {
        public AllEntitiesChangesThisTick EcsChanges;
        public TickId TickId;

        public override string ToString()
        {
            return $"{TickId} : {EcsChanges}";
        }
    }

    public sealed class AllEntitiesChangesEachTickHistory
    {
        readonly Queue<AllEntitiesChangesThisSnapshot> masksQueue = new();

        public AllEntitiesChangesUnionImmutable Fetch(TickIdRange range, ILog log)
        {
            var allEntitiesChangesInRange = masksQueue.Where(mask => range.Contains(mask.TickId)).ToArray();
            var entityChangesMutable = new Dictionary<uint, ComponentFieldMasksMutable>();

            log.Debug("Fetching masks for {Range}", range);
            foreach (var allEntitiesChangesThisTick in allEntitiesChangesInRange)
            {
                foreach (var componentChangesForOneEntity in allEntitiesChangesThisTick.EcsChanges.EntitiesComponentChanges.Values)
                {
                    if (componentChangesForOneEntity.componentChanges.Count == 0)
                    {
                        continue;
                        //throw new Exception("Bad internal state. A changes for one entity but with no component changes makes no sense");
                    }

                    var wasFound = entityChangesMutable.TryGetValue(componentChangesForOneEntity.entityId.Value, out var found);
                    if (!wasFound || found is null)
                    {
                        found = new(range);
                        log.Debug("Had no prior knowledge on {EntityId} so created a new change mutable to it {Change}", componentChangesForOneEntity.entityId, found);
                        entityChangesMutable.Add(componentChangesForOneEntity.entityId.Value, found);
                    }


                    foreach (var component in componentChangesForOneEntity.componentChanges)
                    {
                        log.Debug("merging {Component} with {Mask}", component.Key, component.Value);
                        found.MergeComponentFields(component.Key, component.Value);
                    }

                    log.Debug("result is {Mask}", found);
                }
            }

            var dict = new Dictionary<uint, ComponentChangesForOneEntityImmutable>();
            foreach (var x in entityChangesMutable)
            {
                var immutableChanges =
                    new ComponentChangesForOneEntityImmutable(new((ushort)x.Key), x.Value.FieldMasks);

                dict.Add(x.Key, immutableChanges);
            }

            return new(range, dict);
        }

        public void DiscardUpTo(TickId tickId)
        {
            while (masksQueue.Count > 0)
            {
                if (masksQueue.Peek().TickId.tickId >= tickId.tickId)
                {
                    break;
                }

                masksQueue.Dequeue();
            }
        }

        public void Enqueue(AllEntitiesChangesThisSnapshot componentFieldMasksForTick)
        {
            masksQueue.Enqueue(componentFieldMasksForTick);
        }
    }
}