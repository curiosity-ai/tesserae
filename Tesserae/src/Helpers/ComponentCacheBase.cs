using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// Base class for the keyed <see cref="ComponentCache"/> implementation.
    /// </summary>
    [H5.Name("tss.ComponentCacheBase")]
    public abstract class ComponentCacheBase<TComponent> where TComponent : class
    {
        protected readonly List<(int Key, TComponent Component)> _componentsAndKeys;

        protected ComponentCacheBase()
        {
            _componentsAndKeys = new List<(int Key, TComponent Component)>();
        }

        /// <summary>
        /// Gets or sets the components count.
        /// </summary>
        public int ComponentsCount => _componentsAndKeys.Count;

        /// <summary>
        /// Returns a value indicating whether the component has the given components.
        /// </summary>
        public bool HasComponents => _componentsAndKeys.Any();

        protected void AddToComponents(IEnumerable<TComponent> components)
        {
            var componentsCount = ComponentsCount;

            var componentsToAdd = components.Select((component, index) => new { component, key = index + componentsCount + 1 });

            foreach (var componentToAdd in componentsToAdd)
            {
                _componentsAndKeys.Add((componentToAdd.key, componentToAdd.component));
            }
        }
    }
}