namespace Tesserae
{
    /// <summary>
    /// Sometimes you want a Defer component that will take multiple observable inputs and update when any of them change - but you may also sometimes want one or more of those inputs to actually be fixed inputs (and example is in an editor
    /// form where there is one field that is editable sometimes but not editable other times and you want to observe changes when it's editable but when it ISN'T editable then you don't want to have to change how you initialise your Defer
    /// component - sometimes with three observable inputs and sometimes with four, depending upon whether or not one is readonly). In cases like this, when a non-editable component will be displayed, this may be used to implement the
    /// IObservable interface to pass to Defer - if an EDITABLE version of the input is being used in that particular configuration of the form then there should be an AsObservable method on that component to use instead of this).
    /// </summary>
    [H5.Name("tss.FixedValueObservable")]
    public sealed class FixedValueObservable<TItem> : IObservable<TItem>
    {
        public FixedValueObservable(TItem value) => Value = value;
        public TItem Value { get; }

        // This never changes and so there's no observe-based logic required
        void IObservable<TItem>.Observe(ObservableEvent.ValueChanged<TItem> valueGetter) { }
        void IObservable<TItem>.ObserveFutureChanges(ObservableEvent.ValueChanged<TItem> valueGetter) { }
        void IObservable<TItem>.StopObserving(ObservableEvent.ValueChanged<TItem> valueGetter) { }
    }
}