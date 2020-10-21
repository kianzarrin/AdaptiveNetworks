namespace PrefabMetadata.API {
    using System;
    using System.Collections.Generic;

    public interface IInfoExtended {
        List<ICloneable> MetaData { get; set; }
    }

    public interface IInfoExtended<T> where T : class {
        IInfoExtended<T> Clone();
        T RolledBackClone();
    }
}

