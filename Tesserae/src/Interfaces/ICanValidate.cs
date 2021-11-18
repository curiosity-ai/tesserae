namespace Tesserae
{
    [H5.Name("tss.ICVT")]
    public interface ICanValidate<T> : ICanValidate where T : IComponent
    {
        void Attach(ComponentEventHandler<T> handler);
    }

    [H5.Name("tss.ICV")]
    public interface ICanValidate : IComponent
    {
        string Error { get; set; }
        bool IsInvalid { get; set; }
    }
}