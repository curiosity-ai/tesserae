namespace Tesserae
{
    // 2020-06-30 DWR: It would make sense for TEventArgs to be constrained to being derived from the Event base class but that causes Bridge/h5 issues if the derived type is a generic class - see https://deck.net/205b750f72c11c6302692d62ee85785e/three
    // for a min repro case of the runtime failure to expect

    /// <summary>
    /// Represents a delegate that handles component events with additional event arguments.
    /// Sometimes, component events require additional information other than the type of event (which the subscriber is aware of as they attached to it) and the source component - in which case, that information may be provided via the 'e' parameter
    /// in this delegate. If no such information is required then it makes more sense to use the ComponentEventHandler overload that takes only a single 'sender' parameter.
    /// </summary>
    /// <typeparam name="TSender">The type of the component that is raising the event.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event arguments.</typeparam>
    /// <param name="sender">The component raising the event.</param>
    /// <param name="e">The event arguments.</param>
    public delegate void ComponentEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) where TSender : IComponent;

    /// <summary>
    /// Represents a delegate that handles component events.
    /// If component events do not require any additional information other than the type of event (which the subscriber is aware of as they attached to it) and the source component then this overload should be used, otherwise the overload that takes
    /// an additional 'e' parameter should be used.
    /// </summary>
    /// <typeparam name="TSender">The type of the component that is raising the event.</typeparam>
    /// <param name="sender">The component raising the event.</param>
    public delegate void ComponentEventHandler<TSender>(TSender sender) where TSender : IComponent;
}