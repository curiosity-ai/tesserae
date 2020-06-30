namespace Tesserae.Components
{
    // 2020-06-30 DWR: It would make sense for TEventArgs to be constrained to being derived from the Event base class but that causes Bridge/h5 issues if the derived type is a generic class - see https://deck.net/205b750f72c11c6302692d62ee85785e/three
    // for a min repro case of the runtime failure to expect
    public delegate void ComponentEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) where TSender : IComponent;
}                                                                                                                                        