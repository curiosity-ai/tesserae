namespace Tesserae
{
    public delegate bool BeforeSelectEventHandler<TSender>(TSender sender) where TSender : IComponent;
}