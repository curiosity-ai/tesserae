using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.IObservableComponent")]
    public interface IObservableComponent<T>
    {
        IObservable<T> AsObservable();
    }

    [H5.Name("tss.IObservableListComponent")]
    public interface IObservableListComponent<T>
    {
        IObservable<IReadOnlyList<T>> AsObservable();
    }
}