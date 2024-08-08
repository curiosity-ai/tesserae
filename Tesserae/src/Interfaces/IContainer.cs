using System;
using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.ICBT")]
    public interface IContainerBase<TChild> : IComponent where TChild : IComponent
    {
        void Add(TChild component);
        void Clear();
        void Replace(TChild newComponent, TChild oldComponent);
    }

    [H5.Name("tss.ICBTTC")]
    public interface IContainer<T, TChild> : IContainerBase<TChild> where T : IContainer<T, TChild> where TChild : IComponent
    {
    }

    [H5.Name("tss.ICTX")]
    public static class IContainerExtensions
    {
        public static T Children<T>(this T container, IComponent first, IEnumerable<IComponent> children) where T : IContainer<T, IComponent>
        {
            container.Clear();
            container.Add(first);

            foreach (var x in children)
            {
                container.Add(x);
            }

            return container;
        }

        public static T Children<T>(this T container, IEnumerable<IComponent> children, IComponent last = null) where T : IContainer<T, IComponent>
        {
            container.Clear();

            foreach (var x in children)
            {
                container.Add(x);
            }

            if (last is object)
            {
                container.Add(last);
            }

            return container;
        }

        public static T Children<T>(this T container, IComponent first, IEnumerable<IComponent> children, IComponent last) where T : IContainer<T, IComponent>
        {
            container.Clear();

            if (first is object)
            {
                container.Add(first);
            }

            foreach (var x in children)
            {
                container.Add(x);
            }

            if (last is object)
            {
                container.Add(last);
            }
            return container;
        }

        public static T Children<T>(this T container, params IComponent[] children) where T : IContainer<T, IComponent>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static T Children<T>(this T container, IEnumerable<Nav.NavLink> children) where T : IContainer<Nav.NavLink, Nav.NavLink>
        {
            container.Clear();

            foreach (var x in children)
            {
                container.Add(x);
            }

            return container;
        }

        public static T Children<T>(this T container, params Nav.NavLink[] children) where T : IContainer<Nav.NavLink, Nav.NavLink>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }

        public static T Children<T>(this T container, IEnumerable<ChoiceGroup.Choice> children) where T : IContainer<T, ChoiceGroup.Choice>
        {
            container.Clear();

            foreach (var x in children)
            {
                container.Add(x);
            }

            return container;
        }

        public static T Children<T>(this T container, params ChoiceGroup.Choice[] children) where T : IContainer<T, ChoiceGroup.Choice>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }
    }
}