/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

﻿using System;

namespace Piot.EcsReplicator.Exceptions
{
    public sealed class DeserializeException : Exception
    {
        public DeserializeException(string something) : base(something)
        {
        }
    }
}