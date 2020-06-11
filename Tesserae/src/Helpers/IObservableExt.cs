namespace Tesserae
{
    public static class IObservableExt // 2020-06-11 DWR: I renamed this to "IObservableExtensions" (for consistency with elsewhere and to avoid abbrevs, that tend to be inconsistent) and it caused an error in Mosaik.Front relating to name resolution.. so let's leave it at "IObservableExt"!
    {
        public static void Observe<T>(this IObservable<T> observable, ObservableEvent.ValueChanged<T> valueGetter)
        {
            observable.onValueChanged += valueGetter;
            valueGetter(observable.Value);
        }
    }
}