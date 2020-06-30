using static H5.Core.dom;

namespace Tesserae.Components
{
    public interface ICanValidate<T> : ICanValidate where T : IComponent
    {
        void Attach(ComponentEventHandler<T, Event> handler, Validation.Mode mode);
    }

    public interface ICanValidate : IComponent
    {
        string Error { get; set; }
        bool IsInvalid { get; set; }
    }
}