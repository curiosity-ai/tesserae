using System;

namespace Tesserae.Components
{
    public interface IContainer<T> : IComponent where T : IContainer<T>
    {
        void Add(IComponent component);
        void Clear();
        void Replace(IComponent newComponent, IComponent oldComponent);
    }

    public static class IContainerExtensions
    {
        public static T Children<T>(this T container, params IComponent[] children) where T : IContainer<T>
        {
            children.ForEach(x => container.Add(x));
            return container;
        }
    }
}
