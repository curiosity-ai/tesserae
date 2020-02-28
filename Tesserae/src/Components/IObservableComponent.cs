using System.Collections.Generic;

namespace Tesserae
{
    internal interface IObservableComponent<T>
    {
        IObservable<T> AsObservable();
    }

    internal interface IObservableListComponent<T>
    {
        IObservable<IReadOnlyList<T>> AsObservable();
    }
}