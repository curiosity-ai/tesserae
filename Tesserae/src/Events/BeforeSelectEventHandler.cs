namespace Tesserae
{
    /// <summary>
    /// Represents a delegate that handles an event before a selection occurs,
    /// allowing the selection to be cancelled by returning false.
    /// </summary>
    /// <typeparam name="TSender">The type of the component that is raising the event.</typeparam>
    /// <param name="sender">The component raising the event.</param>
    /// <returns>True to allow the selection, false to cancel it.</returns>
    public delegate bool BeforeSelectEventHandler<TSender>(TSender sender) where TSender : IComponent;
}