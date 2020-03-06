namespace Tesserae
{
    /// <summary>
    /// This allows us to write components that need to update their state when the current route changes. For applications where clicking a link in a navigation menu results in the entire UI being redrawn, this will probably not be
    /// necessary (because the full redraw will result in the navigation menu being redrawn and highlighting the currently-selected item that the User clicked on). However, if a UI has a navigation menu outside of the area of the UI
    /// that will be redrawn when the route changes then the links in that menu may need to listen for route changes and to change their currently-selected state appropriately (this may be seen in the Tesserae Samples application,
    /// when the page is first loaded at a route other than the home page, the sidebar Nav Links receive a message when the route is matched and they ensure that their selected / not-selected states are correct for the route).
    /// </summary>
    public static class RouterObserver
    {
        private static readonly ObserverForAnyRouteChange _anyRouteChangeObserver = new ObserverForAnyRouteChange();

        public static Observable<bool> ForRouteByName(string name)
        {
            var specificObservable = new SettableObservable<bool>();
            _anyRouteChangeObserver.ObserveLazy(newRouteState => specificObservable.Value = newRouteState.name == name);
            return specificObservable;
        }

        private sealed class ObserverForAnyRouteChange : Observable<ActionContext>
        {
            public ObserverForAnyRouteChange() => Router.OnNavigated((toState, _) => Value = toState);
        }
    }
}