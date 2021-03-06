﻿using System;

namespace Lokad.Cqrs.AtomicStorage
{
    public static class NuclearStorageExtensions
    {
        public static TSingleton UpdateSingleton<TSingleton>(this NuclearStorage storage, Action<TSingleton> update)
            where TSingleton : new()
        {
            return storage.Factory.GetEntityWriter<unit,TSingleton>().UpdateEnforcingNew(unit.it,update);
        }
    }

    
}