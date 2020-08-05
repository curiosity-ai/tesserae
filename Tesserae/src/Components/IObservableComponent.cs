using System.Collections.Generic;

namespace Tesserae
{
    public interface IObservableComponent<T>
    {
        IObservable<T> Observable { get; }
    }

    public interface IObservableListComponent<T>
    {
        IObservable<IReadOnlyList<T>> ObservableList { get; }
    }

    public interface IBindableComponent<T>
    {
        SettableObservable<T> Observable { get; set; }
    }

    public interface IBindableListComponent<T>
    {
        ObservableList<T> ObservableList { get; set; }
    }

    public static class IObservableComponentExtension
    {
        public static IObservable<T> AsObservable<T>(this IObservableComponent<T> component)
        {
            return component.Observable;
        }

        public static IObservable<IReadOnlyList<T>> AsObservableList<T>(this IObservableListComponent<T> component)
        {
            return component.ObservableList;
        }

        public static IObservable<T> AsObservable<T>(this IBindableComponent<T> component)
        {
            return component.Observable;
        }

        public static TComponent Bind<TComponent, T>(this TComponent component, SettableObservable<T> observable) where TComponent : IBindableComponent<T>
        {
            component.Observable = observable;
            return component;
        }

        public static TComponent Bind<TComponent, T>(this TComponent component, ObservableList<T> observableList) where TComponent : IBindableListComponent<T>
        {
            component.ObservableList = observableList;
            return component;
        }
    }
}