using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Defines a base interface for components that can contain child components.
    /// </summary>
    /// <typeparam name="TChild">The type of the child components.</typeparam>
    [H5.Name("tss.ICBT")]
    public interface IContainerBase<TChild> : IComponent where TChild : IComponent
    {
        /// <summary>Adds a child component to the container.</summary>
        /// <param name="component">The child component to add.</param>
        void Add(TChild component);
        /// <summary>Clears all child components from the container.</summary>
        void Clear();
        /// <summary>Replaces an existing child component with a new one.</summary>
        /// <param name="newComponent">The new child component.</param>
        /// <param name="oldComponent">The old child component to be replaced.</param>
        void Replace(TChild newComponent, TChild oldComponent);
    }

    /// <summary>
    /// Defines an interface for components that can contain child components, supporting a fluent API.
    /// </summary>
    /// <typeparam name="T">The type of the container.</typeparam>
    /// <typeparam name="TChild">The type of the child components.</typeparam>
    [H5.Name("tss.ICBTTC")]
    public interface IContainer<T, TChild> : IContainerBase<TChild> where T : IContainer<T, TChild> where TChild : IComponent
    {
    }

    /// <summary>
    /// Provides extension methods for IContainer instances.
    /// </summary>
    [H5.Name("tss.ICTX")]
    public static class IContainerExtensions
    {
        /// <summary>
        /// Sets the children of the container.
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="first">The first child component.</param>
        /// <param name="children">A collection of additional child components.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Sets the children of the container.
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">A collection of child components.</param>
        /// <param name="last">An optional last child component.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Sets the children of the container.
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="first">The first child component.</param>
        /// <param name="children">A collection of additional child components.</param>
        /// <param name="last">The last child component.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Sets the children of the container.
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">The child components.</param>
        /// <returns>The current instance of the type.</returns>
        public static T Children<T>(this T container, params IComponent[] children) where T : IContainer<T, IComponent>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }

        /// <summary>
        /// Sets the children of the container (specialized for Nav.NavLink).
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">A collection of NavLink children.</param>
        /// <returns>The current instance of the type.</returns>
        public static T Children<T>(this T container, IEnumerable<Nav.NavLink> children) where T : IContainer<Nav.NavLink, Nav.NavLink>
        {
            container.Clear();

            foreach (var x in children)
            {
                container.Add(x);
            }

            return container;
        }

        /// <summary>
        /// Sets the children of the container (specialized for Nav.NavLink).
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">The NavLink children.</param>
        /// <returns>The current instance of the type.</returns>
        public static T Children<T>(this T container, params Nav.NavLink[] children) where T : IContainer<Nav.NavLink, Nav.NavLink>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }

        /// <summary>
        /// Sets the children of the container (specialized for ChoiceGroup.Choice).
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">A collection of Choice children.</param>
        /// <returns>The current instance of the type.</returns>
        public static T Children<T>(this T container, IEnumerable<ChoiceGroup.Choice> children) where T : IContainer<T, ChoiceGroup.Choice>
        {
            container.Clear();

            foreach (var x in children)
            {
                container.Add(x);
            }

            return container;
        }

        /// <summary>
        /// Sets the children of the container (specialized for ChoiceGroup.Choice).
        /// </summary>
        /// <typeparam name="T">The type of the container.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="children">The Choice children.</param>
        /// <returns>The current instance of the type.</returns>
        public static T Children<T>(this T container, params ChoiceGroup.Choice[] children) where T : IContainer<T, ChoiceGroup.Choice>
        {
            container.Clear();
            children.ForEach(x => container.Add(x));
            return container;
        }
    }
}