using System;

namespace Tesserae
{
    /// <summary>
    /// Identical to <see cref="ConstantObservable{T}"/>, which it now derives from.
    /// </summary>
    [Obsolete("Identical to ConstantObservable<T>; use new ConstantObservable<T>(value) instead.")]
    [Transpose.Name("tss.FixedValueObservable")]
    public sealed class FixedValueObservable<TItem> : ConstantObservable<TItem>
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public FixedValueObservable(TItem value) : base(value) { }
    }
}