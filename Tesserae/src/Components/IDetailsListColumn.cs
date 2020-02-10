using System;

namespace Tesserae.Components
{
    public interface IDetailsListColumn<T>
    {
        string Name                      { get; }

        Func<T, IComponent> OnItemRender { get; }

        Action OnColumnClick             { get; }
    }
}
