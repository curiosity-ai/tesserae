using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    public abstract class ComponentCacheBase<TComponent>
        where TComponent : class
    {
        protected readonly List<(int Key, TComponent Component)> ComponentsAndKeys;

        protected ComponentCacheBase()
        {
            ComponentsAndKeys = new List<(int Key, TComponent Component)>();
        }

        public int ComponentsCount => ComponentsAndKeys.Count;

        public bool HasComponents  => ComponentsAndKeys.Count > 0;

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
                ComponentsAndKeys.Add((componentToAdd.key, componentToAdd.component));
            }
        }
    }
}
