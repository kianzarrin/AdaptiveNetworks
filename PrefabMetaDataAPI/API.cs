namespace PrefabMetadata.API {
    using System;
    using System.Collections.Generic;

    public interface IInfoExtended: ICloneable {
        List<ICloneable> MetaData { get; set; }
    }

    public interface IInfoExtended<T>
        : IInfoExtended
        where T : class
    {
        /// <summary>deep clone.</summary>
        IInfoExtended<T> Clone();

        /// <summary>returns a clone with base type T, removing metadata.</summary>
        T UndoExtend();

        /// <summary>returns self as base type T.</summary>
        T Base { get; }
    }
}

