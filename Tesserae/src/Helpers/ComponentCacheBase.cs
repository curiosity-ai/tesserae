using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    public abstract class ComponentCacheBase<TComponent>
        where TComponent : class
    {
        protected readonly List<(int Key, TComponent Component)> Components;

        protected ComponentCacheBase()
        {
            Components = new List<(int Key, TComponent Component)>();
        }

        public int ComponentsCount => Components.Count;

        protected void AddToComponents(IEnumerable<TComponent> components)
        {
            var componentsCount = ComponentsCount;

            var componentsToAdd =
                components
                    .Select((component, index) => new
                    {
                        component,
                        key = index + componentsCount + 1
                    });

            foreach (var componentToAdd in componentsToAdd)
            {
                Components.Add((componentToAdd.key, componentToAdd.component));
            }
        }
    }
}
