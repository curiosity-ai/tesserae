using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Static class containing factory methods for all Tesserae components and various UI helpers.
    /// </summary>
    [H5.Name("tss.UI")]
    public static partial class UI
    {
        static UI()
        {
        }
        /// <summary>
        /// Helper method to capture the current component inline on it's definition, as an out variable
        /// </summary>
        /// <typeparam name="T">Any component implementing <see cref="IComponent"/></typeparam>
        /// <param name="component"></param>
        /// <param name="var">Capture variable</param>
        /// <returns>itself</returns>
        public static T Var<T>(this T component, out T var) where T : IComponent
        {
            var = component;
            return component;
        }

        /// <summary>
        /// Executes an action on the component and returns it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static T Do<T>(this T component, Action<T> action) where T : IComponent
        {
            action(component);
            return component;
        }

        /// <summary>
        /// Conditionally returns a component.
        /// </summary>
        public static IComponent If(bool condition, IComponent ifTrue, IComponent ifFalse = null) => condition ? (ifTrue ?? Raw()) : (ifFalse ?? Raw());

        /// <summary>
        /// Conditionally returns a component created by the provided function.
        /// </summary>
        public static IComponent If(bool condition, Func<IComponent> ifTrue, IComponent ifFalse = null) => condition ? (ifTrue?.Invoke() ?? Raw()) : (ifFalse ?? Raw());

        /// <summary>
        /// Conditionally returns one of two components created by the provided functions.
        /// </summary>
        public static IComponent If(bool condition, Func<IComponent> ifTrue, Func<IComponent> ifFalse) => condition ? (ifTrue?.Invoke() ?? Raw()) : (ifFalse?.Invoke() ?? Raw());

        /// <summary>
        /// Adds an ID to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Id<T>(this T component, string id) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.id = id;
                return component;
            }

            var el = component.Render();
            el.id = id;
            return component;
        }

        /// <summary>
        /// Adds a CSS class to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T Class<T>(this T component, string className) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.classList.add(className);
                return component;
            }

            var el = component.Render();
            el.classList.add(className);
            return component;
        }

        /// <summary>
        /// Remove a CSS class to the element representing the component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T RemoveClass<T>(this T component, string className) where T : IComponent
        {
            if (component is DeferedComponent deferedComponent)
            {
                deferedComponent.Container.classList.remove(className);
                return component;
            }

            var el = component.Render();
            el.classList.remove(className);
            return component;
        }

        /// <summary>
        /// Creates a <see cref="Raw"/> component from an HTML element.
        /// </summary>
        /// <param name="element">HTML element to be wrapped</param>
        /// <returns>A new <see cref="Raw"/> component wrapping the provided element.</returns>
        public static Raw Raw(HTMLElement element) => new Raw(element);

        /// <summary>
        /// Creates an empty <see cref="Raw"/> component.
        /// </summary>
        public static Raw Raw() => new Raw();

        /// <summary>
        /// Creates a <see cref="Raw"/> component from another component.
        /// </summary>
        public static Raw Raw(IComponent component) => new Raw(component);

        /// <summary>
        /// Creates an empty component.
        /// </summary>
        public static IComponent Empty() => new Raw();

        /// <summary>
        /// Creates an <see cref="Tesserae.Image"/> component.
        /// </summary>
        public static Image Image(string source, string fallback = null) => new Image(source, fallback);

        /// <summary>
        /// Creates a <see cref="Tesserae.Card"/> component.
        /// </summary>
        public static Card Card(IComponent content) => new Card(content);

        /// <summary>
        /// Creates a <see cref="Tesserae.Accordion"/> component.
        /// </summary>
        public static Accordion Accordion(params Expander[] items) => new Accordion(items);

        /// <summary>
        /// Creates a <see cref="Tesserae.Expander"/> component.
        /// </summary>
        public static Expander Expander(string title = null, IComponent content = null) => new Expander(title, content);

        /// <summary>
        /// Creates a <see cref="Tesserae.Badge"/> component.
        /// </summary>
        public static Badge Badge(string text = null) => new Badge(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Tag"/> component.
        /// </summary>
        public static Tag Tag(string text = null) => new Tag(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Chip"/> component.
        /// </summary>
        public static Chip Chip(string text = null) => new Chip(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Avatar"/> component.
        /// </summary>
        public static Avatar Avatar(string image = null, string initials = null) => new Avatar(image, initials);

        /// <summary>
        /// Creates a <see cref="Tesserae.Persona"/> component.
        /// </summary>
        public static Persona Persona(string name = null, string secondaryText = null, string tertiaryText = null, Avatar avatar = null) => new Persona(name, secondaryText, tertiaryText, avatar);

        /// <summary>
        /// Creates a <see cref="Tesserae.Pagination"/> component.
        /// </summary>
        public static Pagination Pagination(int totalItems = 0, int pageSize = 10, int currentPage = 1) => new Pagination(totalItems, pageSize, currentPage);

        /// <summary>
        /// Creates a <see cref="Tesserae.CommandBar"/> component.
        /// </summary>
        public static CommandBar CommandBar(params IComponent[] items) => new CommandBar(items);

        /// <summary>
        /// Creates a <see cref="Tesserae.CommandPalette"/> component.
        /// </summary>
        public static CommandPalette CommandPalette(params CommandPaletteAction[] actions) => new CommandPalette(actions);

        /// <summary>
        /// Creates a <see cref="Tesserae.CommandPaletteAction"/> definition.
        /// </summary>
        public static CommandPaletteAction CommandPaletteAction(string id, string name, Action perform = null)
        {
            return new CommandPaletteAction(id, name) { Perform = perform };
        }

        /// <summary>
        /// Creates a <see cref="Tesserae.CommandBarItem"/> component.
        /// </summary>
        public static CommandBarItem CommandBarItem(string text = null, UIcons? icon = null) => new CommandBarItem(text, icon);

        /// <summary>
        /// Creates a <see cref="Tesserae.Skeleton"/> component.
        /// </summary>
        public static Skeleton Skeleton(SkeletonType type = SkeletonType.Line) => new Skeleton(type);

        /// <summary>
        /// Creates a <see cref="Tesserae.Stepper"/> component.
        /// </summary>
        public static Stepper Stepper(params StepperStep[] steps) => new Stepper(steps);

        /// <summary>
        /// Creates a <see cref="Tesserae.StepperStep"/> definition.
        /// </summary>
        public static StepperStep Step(string title, IComponent content, string description = null) => new StepperStep(title, content, description);

        /// <summary>
        /// Creates a <see cref="Tesserae.Carousel"/> component.
        /// </summary>
        public static Carousel Carousel(params IComponent[] slides) => new Carousel(slides);

        /// <summary>
        /// Creates a <see cref="Tesserae.BackgroundArea"/> component.
        /// </summary>
        public static BackgroundArea BackgroundArea(IComponent content) => new BackgroundArea(content);

        //Note: the Defer method with optional loadMessage caused a bridge compiler issue when resolving the method, so we provide here both with and without the loadMessage method


        public static IDefer Defer(Func<Task<IComponent>>                                   asyncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         => DeferedComponent.Create(asyncGenerator);
        public static IDefer Defer<TComponent>(IObservable<TComponent>                      observableComponent) where TComponent : IComponent                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      => DeferedComponent.Observe(observableComponent, c => c.AsTask());
        public static IDefer Defer<T1>(IObservable<T1>                                      o1,             Func<T1, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                                                                                              => DeferedComponent.Observe(o1,                  asyncGenerator);
        public static IDefer Defer<T1, T2>(IObservable<T1>                                  o1,             IObservable<T2>            o2, Func<T1, T2, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                                                           => DeferedComponent.Observe(o1,                  o2, asyncGenerator);
        public static IDefer Defer<T1, T2, T3>(IObservable<T1>                              o1,             IObservable<T2>            o2, IObservable<T3>                o3, Func<T1, T2, T3, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                    => DeferedComponent.Observe(o1,                  o2, o3, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4>(IObservable<T1>                          o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                                                                                                                         => DeferedComponent.Observe(o1,                  o2, o3, o4, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5>(IObservable<T1>                      o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                                                                          => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6>(IObservable<T1>                  o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, IObservable<T6>                            o6, Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                                                                       => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1>              o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, IObservable<T6>                            o6, IObservable<T7>                                o7, Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator)                                                                                                                                                                                                => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1>          o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, IObservable<T6>                            o6, IObservable<T7>                                o7, IObservable<T8>                                    o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator)                                                                                                                                     => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1>      o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, IObservable<T6>                            o6, IObservable<T7>                                o7, IObservable<T8>                                    o8, IObservable<T9>                                        o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator)                                                                      => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, o9, asyncGenerator);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1,             IObservable<T2>            o2, IObservable<T3>                o3, IObservable<T4>                    o4, IObservable<T5>                        o5, IObservable<T6>                            o6, IObservable<T7>                                o7, IObservable<T8>                                    o8, IObservable<T9>                                        o9, IObservable<T10>                                           o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator) => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, o9, o10, asyncGenerator);
        public static IDefer Defer(Func<Task<IComponent>>                                   asyncGenerator, IComponent                 loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    => DeferedComponent.Create(asyncGenerator, loadMessage);
        public static IDefer Defer<T1>(IObservable<T1>                                      o1,             Func<T1, Task<IComponent>> asyncGenerator, IComponent                     loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     => DeferedComponent.Observe(o1, asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2>(IObservable<T1>                                  o1,             IObservable<T2>            o2,             Func<T1, T2, Task<IComponent>> asyncGenerator, IComponent                         loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  => DeferedComponent.Observe(o1, o2,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3>(IObservable<T1>                              o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             Func<T1, T2, T3, Task<IComponent>> asyncGenerator, IComponent                             loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                           => DeferedComponent.Observe(o1, o2,             o3,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4>(IObservable<T1>                          o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             Func<T1, T2, T3, T4, Task<IComponent>> asyncGenerator, IComponent                                 loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                => DeferedComponent.Observe(o1, o2,             o3,             o4,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5>(IObservable<T1>                      o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             Func<T1, T2, T3, T4, T5, Task<IComponent>> asyncGenerator, IComponent                                     loadMessage)                                                                                                                                                                                                                                                                                                                                 => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6>(IObservable<T1>                  o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             IObservable<T6>                            o6,             Func<T1, T2, T3, T4, T5, T6, Task<IComponent>> asyncGenerator, IComponent                                         loadMessage)                                                                                                                                                                                                                                                              => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             o6,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1>              o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             IObservable<T6>                            o6,             IObservable<T7>                                o7,             Func<T1, T2, T3, T4, T5, T6, T7, Task<IComponent>> asyncGenerator, IComponent                                             loadMessage)                                                                                                                                                                                       => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             o6,             o7,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1>          o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             IObservable<T6>                            o6,             IObservable<T7>                                o7,             IObservable<T8>                                    o8,             Func<T1, T2, T3, T4, T5, T6, T7, T8, Task<IComponent>> asyncGenerator, IComponent                                                 loadMessage)                                                                                                            => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             o6,             o7,             o8,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1>      o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             IObservable<T6>                            o6,             IObservable<T7>                                o7,             IObservable<T8>                                    o8,             IObservable<T9>                                        o9,             Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, Task<IComponent>> asyncGenerator, IComponent                                                      loadMessage)                            => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             o6,             o7,             o8,             o9,             asyncGenerator, loadMessage);
        public static IDefer Defer<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1,             IObservable<T2>            o2,             IObservable<T3>                o3,             IObservable<T4>                    o4,             IObservable<T5>                        o5,             IObservable<T6>                            o6,             IObservable<T7>                                o7,             IObservable<T8>                                    o8,             IObservable<T9>                                        o9,             IObservable<T10>                                           o10,            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, Task<IComponent>> asyncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2,             o3,             o4,             o5,             o6,             o7,             o8,             o9,             o10,            asyncGenerator, loadMessage);

        public static IDefer DeferSync(Func<IComponent>                                         syncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                                                                            => DeferedComponent.Create(() => syncGenerator().AsTask());
        public static IDefer DeferSync<TComponent>(IObservable<TComponent>                      observableComponent) where TComponent : IComponent                                                                                                                                                                                                                                                                                                                                                                                                                        => DeferedComponent.Observe(observableComponent, c => c.AsTask());
        public static IDefer DeferSync<T1>(IObservable<T1>                                      o1,            Func<T1, IComponent> syncGenerator)                                                                                                                                                                                                                                                                                                                                                                                                                        => DeferedComponent.Observe(o1,                  (oc1) => syncGenerator(oc1).AsTask());
        public static IDefer DeferSync<T1, T2>(IObservable<T1>                                  o1,            IObservable<T2>      o2, Func<T1, T2, IComponent> syncGenerator)                                                                                                                                                                                                                                                                                                                                                                                           => DeferedComponent.Observe(o1,                  o2, (oc1,                                  oc2) => syncGenerator(oc1,                                          oc2).AsTask());
        public static IDefer DeferSync<T1, T2, T3>(IObservable<T1>                              o1,            IObservable<T2>      o2, IObservable<T3>          o3, Func<T1, T2, T3, IComponent> syncGenerator)                                                                                                                                                                                                                                                                                                                                                          => DeferedComponent.Observe(o1,                  o2, o3, (oc1,                              oc2, oc3) => syncGenerator(oc1,                                     oc2, oc3).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4>(IObservable<T1>                          o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, Func<T1, T2, T3, T4, IComponent> syncGenerator)                                                                                                                                                                                                                                                                                                                     => DeferedComponent.Observe(o1,                  o2, o3, o4, (oc1,                          oc2, oc3, oc4) => syncGenerator(oc1,                                oc2, oc3, oc4).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5>(IObservable<T1>                      o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, Func<T1, T2, T3, T4, T5, IComponent> syncGenerator)                                                                                                                                                                                                                                                                            => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, (oc1,                      oc2, oc3, oc4, oc5) => syncGenerator(oc1,                           oc2, oc3, oc4, oc5).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6>(IObservable<T1>                  o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, IObservable<T6>                      o6, Func<T1, T2, T3, T4, T5, T6, IComponent> syncGenerator)                                                                                                                                                                                                                               => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, (oc1,                  oc2, oc3, oc4, oc5, oc6) => syncGenerator(oc1,                      oc2, oc3, oc4, oc5, oc6).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1>              o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, IObservable<T6>                      o6, IObservable<T7>                          o7, Func<T1, T2, T3, T4, T5, T6, T7, IComponent> syncGenerator)                                                                                                                                                                              => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, (oc1,              oc2, oc3, oc4, oc5, oc6, oc7) => syncGenerator(oc1,                 oc2, oc3, oc4, oc5, oc6, oc7).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1>          o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, IObservable<T6>                      o6, IObservable<T7>                          o7, IObservable<T8>                              o8, Func<T1, T2, T3, T4, T5, T6, T7, T8, IComponent> syncGenerator)                                                                                                                         => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, (oc1,          oc2, oc3, oc4, oc5, oc6, oc7, oc8) => syncGenerator(oc1,            oc2, oc3, oc4, oc5, oc6, oc7, oc8).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1>      o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, IObservable<T6>                      o6, IObservable<T7>                          o7, IObservable<T8>                              o8, IObservable<T9>                                  o9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IComponent> syncGenerator)                                                                => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, o9, (oc1,      oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9) => syncGenerator(oc1,       oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9).AsTask());
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1,            IObservable<T2>      o2, IObservable<T3>          o3, IObservable<T4>              o4, IObservable<T5>                  o5, IObservable<T6>                      o6, IObservable<T7>                          o7, IObservable<T8>                              o8, IObservable<T9>                                  o9, IObservable<T10>                                     o10, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IComponent> syncGenerator) => DeferedComponent.Observe(o1,                  o2, o3, o4, o5, o6, o7, o8, o9, o10, (oc1, oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9, oc10) => syncGenerator(oc1, oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9, oc10).AsTask());
        public static IDefer DeferSync(Func<IComponent>                                         syncGenerator, IComponent           loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    => DeferedComponent.Create(() => syncGenerator().AsTask(), loadMessage);
        public static IDefer DeferSync<T1>(IObservable<T1>                                      o1,            Func<T1, IComponent> syncGenerator, IComponent               loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            => DeferedComponent.Observe(o1, (oc1) => syncGenerator(oc1).AsTask(), loadMessage);
        public static IDefer DeferSync<T1, T2>(IObservable<T1>                                  o1,            IObservable<T2>      o2,            Func<T1, T2, IComponent> syncGenerator, IComponent                   loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                                                                => DeferedComponent.Observe(o1, o2,                                   (oc1,                                  oc2) => syncGenerator(oc1,                                          oc2).AsTask(),                                          loadMessage);
        public static IDefer DeferSync<T1, T2, T3>(IObservable<T1>                              o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            Func<T1, T2, T3, IComponent> syncGenerator, IComponent                       loadMessage)                                                                                                                                                                                                                                                                                                                                                                                                                => DeferedComponent.Observe(o1, o2,                                   o3, (oc1,                              oc2, oc3) => syncGenerator(oc1,                                     oc2, oc3).AsTask(),                                     loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4>(IObservable<T1>                          o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            Func<T1, T2, T3, T4, IComponent> syncGenerator, IComponent                           loadMessage)                                                                                                                                                                                                                                                                                                                                                            => DeferedComponent.Observe(o1, o2,                                   o3, o4, (oc1,                          oc2, oc3, oc4) => syncGenerator(oc1,                                oc2, oc3, oc4).AsTask(),                                loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5>(IObservable<T1>                      o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            Func<T1, T2, T3, T4, T5, IComponent> syncGenerator, IComponent                               loadMessage)                                                                                                                                                                                                                                                                                                    => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, (oc1,                      oc2, oc3, oc4, oc5) => syncGenerator(oc1,                           oc2, oc3, oc4, oc5).AsTask(),                           loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6>(IObservable<T1>                  o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            IObservable<T6>                      o6,            Func<T1, T2, T3, T4, T5, T6, IComponent> syncGenerator, IComponent                                   loadMessage)                                                                                                                                                                                                                                        => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, o6, (oc1,                  oc2, oc3, oc4, oc5, oc6) => syncGenerator(oc1,                      oc2, oc3, oc4, oc5, oc6).AsTask(),                      loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7>(IObservable<T1>              o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            IObservable<T6>                      o6,            IObservable<T7>                          o7,            Func<T1, T2, T3, T4, T5, T6, T7, IComponent> syncGenerator, IComponent                                       loadMessage)                                                                                                                                                                        => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, o6, o7, (oc1,              oc2, oc3, oc4, oc5, oc6, oc7) => syncGenerator(oc1,                 oc2, oc3, oc4, oc5, oc6, oc7).AsTask(),                 loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8>(IObservable<T1>          o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            IObservable<T6>                      o6,            IObservable<T7>                          o7,            IObservable<T8>                              o8,            Func<T1, T2, T3, T4, T5, T6, T7, T8, IComponent> syncGenerator, IComponent                                           loadMessage)                                                                                                    => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, o6, o7, o8, (oc1,          oc2, oc3, oc4, oc5, oc6, oc7, oc8) => syncGenerator(oc1,            oc2, oc3, oc4, oc5, oc6, oc7, oc8).AsTask(),            loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IObservable<T1>      o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            IObservable<T6>                      o6,            IObservable<T7>                          o7,            IObservable<T8>                              o8,            IObservable<T9>                                  o9,            Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IComponent> syncGenerator, IComponent                                                loadMessage)                           => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, o6, o7, o8, o9, (oc1,      oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9) => syncGenerator(oc1,       oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9).AsTask(),       loadMessage);
        public static IDefer DeferSync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IObservable<T1> o1,            IObservable<T2>      o2,            IObservable<T3>          o3,            IObservable<T4>              o4,            IObservable<T5>                  o5,            IObservable<T6>                      o6,            IObservable<T7>                          o7,            IObservable<T8>                              o8,            IObservable<T9>                                  o9,            IObservable<T10>                                     o10,           Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IComponent> syncGenerator, IComponent loadMessage) => DeferedComponent.Observe(o1, o2,                                   o3, o4, o5, o6, o7, o8, o9, o10, (oc1, oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9, oc10) => syncGenerator(oc1, oc2, oc3, oc4, oc5, oc6, oc7, oc8, oc9, oc10).AsTask(), loadMessage);

        /// <summary>
        /// A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.
        /// </summary>
        public static Stack Stack(Stack.Orientation orientation = Tesserae.Stack.Orientation.Vertical) => new Stack(orientation);

        /// <summary>
        /// Creates a <see cref="Tesserae.Masonry"/> component.
        /// </summary>
        public static Masonry Masonry(int columns, int gutter = 10) => new Masonry(columns: columns, gutter: gutter);

        /// <summary>
        /// Creates a horizontal <see cref="Tesserae.Stack"/> component.
        /// </summary>
        /// <returns></returns>
        public static Stack HStack() => new Stack(Tesserae.Stack.Orientation.Horizontal);

        /// <summary>
        /// Creates a vertical <see cref="Tesserae.Stack"/> component.
        /// </summary>
        /// <returns></returns>
        public static Stack VStack() => new Stack(Tesserae.Stack.Orientation.Vertical);
        
        /// <summary>
        /// Creates a <see cref="Tesserae.SortableStack"/> component.
        /// </summary>
        public static SortableStack SortableStack(Stack.Orientation orientation = Tesserae.Stack.Orientation.Vertical) => new SortableStack(orientation);
        /// <summary>
        /// Creates a vertical <see cref="Tesserae.SortableStack"/> component.
        /// </summary>
        public static SortableStack VSortableStack() => new SortableStack(Tesserae.Stack.Orientation.Vertical);
        /// <summary>
        /// Creates a horizontal <see cref="Tesserae.SortableStack"/> component.
        /// </summary>
        public static SortableStack HSortableStack() => new SortableStack(Tesserae.Stack.Orientation.Horizontal);

        /// <summary>
        /// Creates a <see cref="Tesserae.Grid"/> component with the specified columns.
        /// </summary>
        public static Grid Grid(params UnitSize[] columns)                  => new Grid(columns);
        /// <summary>
        /// Creates a <see cref="Tesserae.Grid"/> component with the specified columns and rows.
        /// </summary>
        public static Grid Grid(UnitSize[]        columns, UnitSize[] rows) => new Grid(columns, rows);

        /// <summary>
        /// Creates a <see cref="Tesserae.SectionStack"/> component.
        /// </summary>
        public static SectionStack SectionStack() => new SectionStack();

        /// <summary>
        /// Creates a <see cref="Tesserae.Float"/> component.
        /// </summary>
        public static Float Float(IComponent child, Float.Position position) => new Float(child, position);

        /// <summary>
        /// Creates a <see cref="Tesserae.Button"/> component.
        /// </summary>
        public static Button Button(string text = string.Empty) => new Button(text);
        public static ButtonAndIcon ButtonAndIcon(string text, ButtonAndIcon.IconClickHandler onIconClick, UIcons mainIcon = UIcons.Circle, UIcons secondaryIcon = UIcons.AngleDown) => new ButtonAndIcon(text, onIconClick, mainIcon, secondaryIcon);
        public static ActionButton ActionButton(string     textContent, UIcons displayIcon,                         UIconsWeight displayIconWeight = UIconsWeight.Regular, string   displayColor   = null, TextSize displayIconSize = TextSize.Small, UIconsWeight actionIconWeight = UIconsWeight.Regular, UIcons actionIcon = UIcons.AngleCircleDown, string actionColor = null, TextSize actionIconSize = TextSize.Small) => new ActionButton(textContent, displayIcon, displayIconWeight, displayColor, displayIconSize, actionIconWeight, actionIcon, actionColor, actionIconSize);
        public static ActionButton ActionButton(string     textContent, UIcons actionIcon = UIcons.AngleCircleDown, UIconsWeight actionIconWeight  = UIconsWeight.Regular, string   actionColor    = null, TextSize actionIconSize  = TextSize.Small) => new ActionButton(textContent, actionIcon: actionIcon, actionIconWeight: actionIconWeight, actionColor: actionColor, actionIconSize: actionIconSize);
        public static ActionButton ActionButton(IComponent content,    string actionIcon = null,                   string       actionColor       = null,                 TextSize actionIconSize = TextSize.Small) => new ActionButton(content, actionIcon, actionColor, actionIconSize);

        /// <summary>
        /// Creates a <see cref="Tesserae.CheckBox"/> component.
        /// </summary>
        public static CheckBox CheckBox(string text = string.Empty) => new CheckBox(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Toggle"/> component.
        /// </summary>
        public static Toggle Toggle(IComponent onText, IComponent offText) => new Toggle(onText: onText, offText: offText);

        /// <summary>
        /// Creates a <see cref="Tesserae.Toggle"/> component.
        /// </summary>
        public static Toggle Toggle(string onText, string offText) => new Toggle(onText: TextBlock(onText), offText: TextBlock(offText));

        /// <summary>
        /// Creates a <see cref="Tesserae.Toggle"/> component.
        /// </summary>
        public static Toggle Toggle(string text) => new Toggle(onText: TextBlock(text), offText: TextBlock(text).Secondary());

        /// <summary>
        /// Creates a <see cref="Tesserae.Toggle"/> component.
        /// </summary>
        public static Toggle Toggle() => new Toggle(null, null);

        /// <summary>
        /// Creates a <see cref="Tesserae.ChoiceGroup.Choice"/> component.
        /// </summary>
        public static ChoiceGroup.Choice Choice(string label = string.Empty) => new ChoiceGroup.Choice(label);

        /// <summary>
        /// Creates a <see cref="Tesserae.ChoiceGroup"/> component.
        /// </summary>
        public static ChoiceGroup ChoiceGroup(string label = string.Empty) => new ChoiceGroup(label);

        /// <summary>
        /// Creates a <see cref="Tesserae.TextBlock"/> component.
        /// </summary>
        public static TextBlock TextBlock(string text)                                                                                                                                                                            => new TextBlock(text);
        /// <summary>
        /// Creates a <see cref="Tesserae.TextBlock"/> component.
        /// </summary>
        public static TextBlock TextBlock(string text = string.Empty, bool treatAsHTML = false, bool selectable = false, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular, string afterText = null) => new TextBlock(text, treatAsHTML, selectable, textSize, textWeight, afterText: afterText);

        /// <summary>
        /// Creates a <see cref="Tesserae.FileSelector"/> component.
        /// </summary>
        public static FileSelector FileSelector() => new FileSelector();

        /// <summary>
        /// Creates a <see cref="Tesserae.FileDropArea"/> component.
        /// </summary>
        public static FileDropArea FileDropArea() => new FileDropArea();

        /// <summary>
        /// Creates a <see cref="Tesserae.Validator"/> component.
        /// </summary>
        public static Validator Validator() => new Validator();

        /// <summary>
        /// Creates a <see cref="Tesserae.Icon"/> component.
        /// </summary>
        public static Icon Icon(UIcons  icon, string       color)                                                                              => new Icon(icon).Foreground(color               ?? "");
        /// <summary>
        /// Creates a <see cref="Tesserae.Icon"/> component.
        /// </summary>
        public static Icon Icon(UIcons  icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Small, string color = null) => new Icon(icon, weight, size).Foreground(color ?? "");
        /// <summary>
        /// Creates a <see cref="Tesserae.Icon"/> component.
        /// </summary>
        public static Icon Icon(Emoji   icon, TextSize     size   = TextSize.Medium) => new Icon(icon, size);

        /// <summary>
        /// Creates a <see cref="Tesserae.HorizontalSeparator"/> component.
        /// </summary>
        public static HorizontalSeparator HorizontalSeparator(string text) => new HorizontalSeparator(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.HorizontalSeparator"/> component.
        /// </summary>
        public static HorizontalSeparator HorizontalSeparator(IComponent component) => new HorizontalSeparator(component);

        /// <summary>
        /// Creates a <see cref="Tesserae.Label"/> component.
        /// </summary>
        public static Label Label(string text = string.Empty) => new Label(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Label"/> component.
        /// </summary>
        public static Label Label(IComponent component) => new Label(component);

        /// <summary>
        /// Creates an <see cref="Tesserae.EditableLabel"/> component.
        /// </summary>
        public static EditableLabel EditableLabel(string text = string.Empty) => new EditableLabel(text);

        /// <summary>
        /// Creates an <see cref="Tesserae.EditableArea"/> component.
        /// </summary>
        public static EditableArea EditableArea(string text = string.Empty) => new EditableArea(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Breadcrumb"/> component.
        /// </summary>
        public static Breadcrumb Breadcrumb() => new Breadcrumb();

        /// <summary>
        /// Creates a <see cref="Tesserae.TextBreadcrumbs"/> component.
        /// </summary>
        public static TextBreadcrumbs TextBreadcrumbs() => new TextBreadcrumbs();

        /// <summary>
        /// Creates a <see cref="Tesserae.TextBreadcrumb"/> component.
        /// </summary>
        public static TextBreadcrumb TextBreadcrumb(string text = string.Empty) => new TextBreadcrumb(text);

        /// <summary>
        /// Creates a crumb for breadcrumbs.
        /// </summary>
        public static Button Crumb(string text = string.Empty) => new Button(text).NoBorder().NoBackground();

        /// <summary>
        /// Creates a <see cref="Tesserae.OverflowSet"/> component.
        /// </summary>
        public static OverflowSet OverflowSet() => new OverflowSet();

        /// <summary>
        /// Creates a <see cref="Tesserae.TextBox"/> component.
        /// </summary>
        public static TextBox TextBox(string text = string.Empty) => new TextBox(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.TextArea"/> component.
        /// </summary>
        public static TextArea TextArea(string text = string.Empty) => new TextArea(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.ColorPicker"/> component.
        /// </summary>
        public static ColorPicker ColorPicker(Color color = null) => new ColorPicker(color);

        /// <summary>
        /// Creates a <see cref="Tesserae.DateTimePicker"/> component.
        /// </summary>
        public static DateTimePicker DateTimePicker(DateTime? dateTime = null) => new DateTimePicker(dateTime);

        /// <summary>
        /// Creates a <see cref="Tesserae.DatePicker"/> component.
        /// </summary>
        public static DatePicker DatePicker(DateTime? dateTime = null) => new DatePicker(dateTime);

        /// <summary>
        /// Creates a <see cref="Tesserae.NumberPicker"/> component.
        /// </summary>
        public static NumberPicker NumberPicker(int defaultValue = 0) => new NumberPicker(defaultValue);

        /// <summary>
        /// Creates a <see cref="Tesserae.CronEditor"/> component.
        /// </summary>
        public static CronEditor CronEditor(string initialCron = "0 12 * * *") => new CronEditor(initialCron);

        /// <summary>
        /// Creates a <see cref="Tesserae.GridPicker"/> component.
        /// </summary>
        public static GridPicker GridPicker(string[] columnNames, string[] rowNames, int states, int[][] initialStates, Action<Button, int, int> formatState, UnitSize[] columns = null, UnitSize rowHeight = null) => new GridPicker(columnNames, rowNames, states, initialStates, formatState, columns, rowHeight);

        /// <summary>
        /// Creates a <see cref="Tesserae.SearchBox"/> component.
        /// </summary>
        public static SearchBox SearchBox(string placeholder = string.Empty) => new SearchBox(placeholder);

        /// <summary>
        /// Creates a <see cref="Tesserae.Slider"/> component.
        /// </summary>
        public static Slider Slider(int val = 0, int min = 0, int max = 100, int step = 10) => new Slider(val, min, max, step);

        /// <summary>
        /// A Layer is a technical component that does not have specific Design guidance.
        ///
        /// Layers are used to render content outside of a DOM tree, at the end of the document. This allows content to escape traditional boundaries caused by "overflow: hidden" css rules and keeps it on the top without using z-index rules. This is useful for example in
        /// ContextualMenu and Tooltip scenarios, where the content should always overlay everything else.
        /// </summary>
        public static Layer Layer() => new Layer();

        /// <summary>
        /// Creates a <see cref="Tesserae.LayerHost"/> component.
        /// </summary>
        public static LayerHost LayerHost() => new LayerHost();

        /// <summary>
        /// Creates a <see cref="Tesserae.Nav"/> component.
        /// </summary>
        public static Nav Nav() => new Nav();

        /// <summary>
        /// Creates a <see cref="Tesserae.Nav.NavLink"/> component.
        /// </summary>
        public static Nav.NavLink NavLink(string text = null) => new Nav.NavLink(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Nav.NavLink"/> component.
        /// </summary>
        public static Nav.NavLink NavLink(IComponent content) => new Nav.NavLink(content);

        /// <summary>
        /// Creates a <see cref="Tesserae.Panel"/> component.
        /// </summary>
        public static Panel Panel(string     title = null) => new Panel(title);
        /// <summary>
        /// Creates a <see cref="Tesserae.Panel"/> component.
        /// </summary>
        public static Panel Panel(IComponent title)        => new Panel(title);

        /// <summary>
        /// Creates a <see cref="Tesserae.Modal"/> component.
        /// </summary>
        public static Modal Modal(IComponent header = null) => new Modal(header);

        /// <summary>
        /// Creates a <see cref="Tesserae.Modal"/> component.
        /// </summary>
        public static Modal Modal(string header) => new Modal(string.IsNullOrWhiteSpace(header) ? null : TextBlock(header).SemiBold());

        /// <summary>
        /// Creates a <see cref="Tesserae.TutorialModal"/> component.
        /// </summary>
        public static TutorialModal TutorialModal()                              => new TutorialModal("",    "");
        /// <summary>
        /// Creates a <see cref="Tesserae.TutorialModal"/> component.
        /// </summary>
        public static TutorialModal TutorialModal(string title, string helpText) => new TutorialModal(title, helpText);

        /// <summary>
        /// Creates a <see cref="Tesserae.ProgressModal"/> component.
        /// </summary>
        public static ProgressModal ProgressModal() => new ProgressModal();

        /// <summary>
        /// Creates a <see cref="Tesserae.Dialog"/> component.
        /// </summary>
        public static Dialog Dialog(IComponent content = null, IComponent title = null, bool centerContent = true) => new Dialog(content, title, centerContent);

        /// <summary>
        /// Creates a <see cref="Tesserae.Dialog"/> component.
        /// </summary>
        public static Dialog Dialog(string text, bool centerContent = true) => new Dialog(content: string.IsNullOrWhiteSpace(text) ? null : TextBlock(text).MaxWidth(50.vw()), centerContent: centerContent);
        /// <summary>
        /// Creates a <see cref="Tesserae.Dialog"/> component.
        /// </summary>
        public static Dialog Dialog(string title, string content, bool centerContent = true) => new Dialog(title: string.IsNullOrWhiteSpace(title) ? null : TextBlock(title).MaxWidth(50.vw()), content: string.IsNullOrWhiteSpace(content) ? null : TextBlock(content).MaxWidth(50.vw()), centerContent: centerContent);

        /// <summary>
        /// Creates a <see cref="Tesserae.Pivot"/> component.
        /// </summary>
        public static Pivot Pivot() => new Pivot();

        /// <summary>
        /// Creates a <see cref="Tesserae.PivotSelector"/> component.
        /// </summary>
        public static PivotSelector PivotSelector() => new PivotSelector();

        /// <summary>
        /// Creates a function that returns a component to be used as a pivot title.
        /// </summary>
        public static Func<IComponent> PivotTitle(string text) => () => Button(text).NoBackground().Regular();

        /// <summary>
        /// Creates a function that returns a component to be used as a pivot title.
        /// </summary>
        public static Func<IComponent> PivotTitle(string text, UIcons icon) => () => Button(text).NoBackground().Regular().SetIcon(icon);

        /// <summary>
        /// Creates a <see cref="Tesserae.Sidebar"/> component.
        /// </summary>
        public static Sidebar Sidebar(bool sortable = false) => new Sidebar(sortable);

        //public static Sidebar.Item SidebarItem(string text, string icon, string href = null) => new Sidebar.Item(text, icon, href);

        //public static Sidebar.Item SidebarItem(string     text, IComponent icon, string href = null) => new Sidebar.Item(text, icon, href);
        //public static Sidebar.Item SidebarItem(IComponent text, string     icon, string href = null) => new Sidebar.Item(text, icon, href);
        //public static Sidebar.Item SidebarItem(IComponent text, string     href              = null) => new Sidebar.Item(text, href);
        //public static Sidebar.Item SidebarItem(IComponent text, IComponent icon, string href = null) => new Sidebar.Item(text, icon, href);

        /// <summary>
        /// Creates a <see cref="Tesserae.Toast"/> component.
        /// </summary>
        public static Toast Toast() => new Toast();

        /// <summary>
        /// Creates a <see cref="Tesserae.SavingToast"/> component.
        /// </summary>
        public static SavingToast SavingToast(string initialMessage = null) => new SavingToast(initialMessage);

        /// <summary>
        /// Creates a <see cref="Tesserae.SaveButton"/> component.
        /// </summary>
        public static SaveButton SaveButton() => new SaveButton();

        /// <summary>
        /// Creates a <see cref="Tesserae.ProgressIndicator"/> component.
        /// </summary>
        public static ProgressIndicator ProgressIndicator() => new ProgressIndicator();

        /// <summary>
        /// Creates a <see cref="Tesserae.NodeView"/> component.
        /// </summary>
        public static NodeView NodeView()                            => new NodeView();
        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown"/> component.
        /// </summary>
        public static Dropdown Dropdown()                            => new Dropdown();
        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown"/> component.
        /// </summary>
        public static Dropdown Dropdown(string          noItemsText) => new Dropdown(noItemsSpan: string.IsNullOrWhiteSpace(noItemsText) ? null : Span(_(text: noItemsText)));
        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown"/> component.
        /// </summary>
        public static Dropdown Dropdown(HTMLSpanElement noItemsSpan) => new Dropdown(noItemsSpan);

        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown.Item"/> component.
        /// </summary>
        public static Dropdown.Item DropdownItem() => new Dropdown.Item("");

        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown.Item"/> component.
        /// </summary>
        public static Dropdown.Item DropdownItem(string text, string selectedText = string.Empty) => new Dropdown.Item(text, selectedText);

        /// <summary>
        /// Creates a <see cref="Tesserae.Dropdown.Item"/> component.
        /// </summary>
        public static Dropdown.Item DropdownItem(IComponent content, IComponent selectedContent = null) => new Dropdown.Item(content, selectedContent);

        /// <summary>
        /// Creates a <see cref="Tesserae.ContextMenu"/> component.
        /// </summary>
        public static ContextMenu ContextMenu() => new ContextMenu();

        /// <summary>
        /// Creates a <see cref="Tesserae.ContextMenu.Item"/> component.
        /// </summary>
        public static ContextMenu.Item ContextMenuItem(string text = string.Empty) => new ContextMenu.Item(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.ContextMenu.Item"/> component.
        /// </summary>
        public static ContextMenu.Item ContextMenuItem(IComponent component) => new ContextMenu.Item(component);

        /// <summary>
        /// Creates a <see cref="Tesserae.Spinner"/> component.
        /// </summary>
        public static Spinner Spinner(string text = string.Empty) => new Spinner(text);

        /// <summary>
        /// Creates a <see cref="Tesserae.Link"/> component.
        /// </summary>
        public static Link Link(string url, IComponent content, bool noUnderline = false) => new Link(url, content, noUnderline);

        /// <summary>
        /// Creates a <see cref="Tesserae.Link"/> component.
        /// </summary>
        public static Link Link(string url, string text) => new Link(url, TextBlock(text));

        /// <summary>
        /// Creates a <see cref="Tesserae.Link"/> component.
        /// </summary>
        public static Link Link(string url, string text, UIcons icon, bool noUnderline = false) => new Link(url, Button(text).SetIcon(icon).NoBorder().NoBackground().Padding(0.px()), noUnderline);

        /// <summary>
        /// Creates a <see cref="Tesserae.SplitView"/> component.
        /// </summary>
        public static SplitView           SplitView()           => new SplitView();
        /// <summary>
        /// Creates a <see cref="Tesserae.HorizontalSplitView"/> component.
        /// </summary>
        public static HorizontalSplitView HorizontalSplitView() => new HorizontalSplitView();

        /// <summary>
        /// Creates a <see cref="Tesserae.VirtualizedList"/> component.
        /// </summary>
        public static VirtualizedList VirtualizedList(int rowsPerPage = 4, int columnsPerRow = 4) => new VirtualizedList(rowsPerPage, columnsPerRow);

        /// <summary>
        /// Creates a <see cref="Tesserae.SearchableList{T}"/> component.
        /// </summary>
        public static SearchableList<T> SearchableList<T>(T[] components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.SearchableList{T}"/> component.
        /// </summary>
        public static SearchableList<T> SearchableList<T>(ObservableList<T> components, params UnitSize[] columns) where T : ISearchableItem => new SearchableList<T>(components, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.SearchableGroupedList{T}"/> component.
        /// </summary>
        public static SearchableGroupedList<T> SearchableGroupedList<T>(T[] components, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns) where T : ISearchableGroupedItem => new SearchableGroupedList<T>(components, groupedItemHeaderGenerator, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.SearchableGroupedList{T}"/> component.
        /// </summary>
        public static SearchableGroupedList<T> SearchableGroupedList<T>(ObservableList<T> components, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns) where T : ISearchableGroupedItem => new SearchableGroupedList<T>(components, groupedItemHeaderGenerator, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.ItemsList"/> component.
        /// </summary>
        public static ItemsList ItemsList(IComponent[] components, params UnitSize[] columns) => new ItemsList(components, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.ItemsList"/> component.
        /// </summary>
        public static ItemsList ItemsList(ObservableList<IComponent> components, params UnitSize[] columns) => new ItemsList(components, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.InfiniteScrollingList"/> component.
        /// </summary>
        public static InfiniteScrollingList InfiniteScrollingList(Func<IComponent[]>       getNextItemPage, params UnitSize[]        columns)                                    => new InfiniteScrollingList(getNextItemPage, columns);
        /// <summary>
        /// Creates a <see cref="Tesserae.InfiniteScrollingList"/> component.
        /// </summary>
        public static InfiniteScrollingList InfiniteScrollingList(Func<Task<IComponent[]>> getNextItemPage, params UnitSize[]        columns)                                    => new InfiniteScrollingList(getNextItemPage, columns);
        /// <summary>
        /// Creates a <see cref="Tesserae.InfiniteScrollingList"/> component.
        /// </summary>
        public static InfiniteScrollingList InfiniteScrollingList(IComponent[]             initComponents,  Func<Task<IComponent[]>> getNextItemPage, params UnitSize[] columns) => new InfiniteScrollingList(initComponents,  getNextItemPage, columns);
        /// <summary>
        /// Creates a <see cref="Tesserae.InfiniteScrollingList"/> component.
        /// </summary>
        public static InfiniteScrollingList InfiniteScrollingList(IComponent[]             initComponents,  Func<IComponent[]>       getNextItemPage, params UnitSize[] columns) => new InfiniteScrollingList(initComponents,  getNextItemPage, columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.DetailsList{TDetailsListItem}"/> component.
        /// </summary>
        public static DetailsList<TDetailsListItem> DetailsList<TDetailsListItem>(params IDetailsListColumn[] columns) where TDetailsListItem : class, IDetailsListItem<TDetailsListItem> => new DetailsList<TDetailsListItem>(columns);

        /// <summary>
        /// Creates a <see cref="Tesserae.DetailsListIconColumn"/> component.
        /// </summary>
        public static DetailsListIconColumn IconColumn(Icon icon, UnitSize width, bool     enableColumnSorting = false, string sortingKey          = null,  Action onColumnClick = null)                              => new DetailsListIconColumn(icon, width, null,     enableColumnSorting, sortingKey, onColumnClick);
        /// <summary>
        /// Creates a <see cref="Tesserae.DetailsListIconColumn"/> component.
        /// </summary>
        public static DetailsListIconColumn IconColumn(Icon icon, UnitSize width, UnitSize maxWidth,                    bool   enableColumnSorting = false, string sortingKey    = null, Action onColumnClick = null) => new DetailsListIconColumn(icon, width, maxWidth, enableColumnSorting, sortingKey, onColumnClick);

        /// <summary>
        /// Creates a <see cref="Tesserae.DetailsListColumn"/> component.
        /// </summary>
        public static DetailsListColumn DetailsListColumn(string title, UnitSize width, bool     isRowHeader = false, bool enableColumnSorting = false, string sortingKey          = null,  Action onColumnClick = null)                              => new DetailsListColumn(title, width, null,     isRowHeader, enableColumnSorting, sortingKey, onColumnClick);
        /// <summary>
        /// Creates a <see cref="Tesserae.DetailsListColumn"/> component.
        /// </summary>
        public static DetailsListColumn DetailsListColumn(string title, UnitSize width, UnitSize maxWidth,            bool isRowHeader         = false, bool   enableColumnSorting = false, string sortingKey    = null, Action onColumnClick = null) => new DetailsListColumn(title, width, maxWidth, isRowHeader, enableColumnSorting, sortingKey, onColumnClick);

        /// <summary>
        /// Creates a <see cref="Tesserae.Picker{TPickerItem}"/> component.
        /// </summary>
        public static Picker<TPickerItem> Picker<TPickerItem>(int maximumAllowedSelections = int.MaxValue, bool duplicateSelectionsAllowed = false, int suggestionsTolerance = 0, bool renderSelectionsInline = true, string suggestionsTitleText = null) where TPickerItem : class, IPickerItem => new Picker<TPickerItem>(maximumAllowedSelections, duplicateSelectionsAllowed, suggestionsTolerance, renderSelectionsInline, suggestionsTitleText);

        /// <summary>
        /// Creates a <see cref="Tesserae.VisibilitySensor"/> component.
        /// </summary>
        public static VisibilitySensor VisibilitySensor(Action<VisibilitySensor> onVisible, bool singleCall = true, IComponent message = null) => new VisibilitySensor(onVisible, singleCall, message);

        /// <summary>
        /// Creates a <see cref="Tesserae.CombinedObservable{T1, T2}"/> helper.
        /// </summary>
        public static CombinedObservable<T1, T2> Combine<T1, T2>(IObservable<T1> o1, IObservable<T2> o2) => new CombinedObservable<T1, T2>(o1, o2);

        /// <summary>
        /// Creates a <see cref="Tesserae.Timeline"/> component.
        /// </summary>
        public static Timeline     Timeline()                   => new Timeline();
        /// <summary>
        /// Creates a <see cref="Tesserae.Teaching"/> component.
        /// </summary>
        public static Teaching     Teaching()                   => new Teaching();
        /// <summary>
        /// Converts a <see cref="Tesserae.Button"/> to a <see cref="Tesserae.ToggleButton"/>.
        /// </summary>
        public static ToggleButton ToToggle(this Button button) => new ToggleButton(button);

        /// <summary>
        /// Tries to remove a child element from a parent element.
        /// </summary>
        public static void TryRemoveChild(HTMLElement parentElement, HTMLElement childToRemove)
        {
            if(parentElement.contains(childToRemove))
            {
                parentElement.removeChild(childToRemove);
            }
        }
    }
}
