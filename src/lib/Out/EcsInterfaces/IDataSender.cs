/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿using Piot.Flood;

namespace Piot.EcsReplicator.Out.EcsInterfaces
{
    /// <summary>
    ///     Interfaces for both fetching and serializing Data structures to a bit stream
    /// </summary>
    public interface IDataSender
    {
        public bool HasComponentTypeId(uint entityId, ushort componentTypeId);

        public uint[] AllEntities();
        public void WriteMask(IBitWriter writer, uint entityId, ushort componentTypeId, ulong masks);
        public void WriteFull(IBitWriter writer, uint entityId, ushort componentTypeId);
    }
}
