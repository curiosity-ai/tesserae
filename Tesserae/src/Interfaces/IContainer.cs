using System;

namespace Tesserae.Components
{
    public interface IContainer<T, TChild> : IComponent where T : IContainer<T, TChild> where TChild : IComponent
    {
        void Add(TChild component);
        void Clear();
        void Replace(TChild newComponent, TChild oldComponent);
    }

    public static class IContainerExtensions
    {
        public static T Children<T>(this T container, params IComponent[] children) where T : IContainer<T, IComponent>
        {
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static T Children<T>(this T container, params Nav.NavLink[] children) where T : IContainer<T, Nav.NavLink>
        {
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static T Children<T>(this T container, params ChoiceGroup.Option[] children) where T : IContainer<T, ChoiceGroup.Option>
        {
            children.ForEach(x => container.Add(x));
            return container;
        }
    }
}
