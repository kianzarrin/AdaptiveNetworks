namespace PrefabMetadata.API {
    using System;
    using System.Collections.Generic;
    using System.Collections;

    public interface IInfoExtended: ICloneable {
        List<ICloneable> MetaData { get; set; }
    }

    public interface IInfoExtended<T>
        : IInfoExtended
        where T : class
    {
        IInfoExtended<T> Clone();
        T UndoExtend();
        T Base { get; }
    }
}

