using System;

namespace Tesserae.Components
{
    public interface ICanValidate<T> : ICanValidate
    {
        void Attach(EventHandler<T> handler, Validation.Mode mode);
    }

    public interface ICanValidate
    {
        string Error { get; set; }
        bool IsInvalid { get; set; }
    }
}
