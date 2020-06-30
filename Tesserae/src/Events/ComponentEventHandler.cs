namespace Tesserae.Components
{
    // 2020-06-30 DWR: It would make sense for TEventArgs to be constrained to being derived from the Event base class but that causes Bridge/h5 issues if the derived type is a generic class - see https://deck.net/205b750f72c11c6302692d62ee85785e/three
    // for a min repro case of the runtime failure to expect

    /// <summary>
    /// Sometimes, component events require additional information other than the type of event (which the subscriber is aware of as they attached to it) and the source component - in which case, that information may be provided via the 'e' parameter
    /// in this delegate. If no such information is required then it makes more sense to use the ComponentEventHandler overload that takes only a single 'sender' parameter.
    /// </summary>
    public delegate void ComponentEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) where TSender : IComponent;

    /// <summary>
    /// If component events do not require any additional information other than the type of event (which the subscriber is aware of as they attached to it) and the source component then this overload should be used, otherwise the overload that takes
    /// an additional 'e' parameter should be
    /// </summary>
    public delegate void ComponentEventHandler<TSender>(TSender sender) where TSender : IComponent;
}