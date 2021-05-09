using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H5;
using H5.Core;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    public static class Router
    {
        public delegate void NavigatedHandler(State               toState, State fromState);
        public delegate bool CanNavigateHandler(State             toState, State fromState);
        public delegate void NoMatchHandler(ReadOnlyArray<string> routeParts);

        private static event NavigatedHandler Navigated;
        private static event NoMatchHandler   NotMatched;

        private static State              _currentState;
        private static CanNavigateHandler _beforeNavigate; // 2020-06-16 DWR: We previously used an event for this but only allowed a single delegate to bind to it, so there is no need for it to be multi-dispatch and so now it's just a field instead of an event

        public static void OnBeforeNavigate(CanNavigateHandler onBeforeNavigate) => _beforeNavigate = onBeforeNavigate;
        public static void OnNavigated(NavigatedHandler        onNavigated)      => Navigated += onNavigated;
        public static void OnNotMatched(NoMatchHandler         notMatched)       => NotMatched += notMatched;

        public static void Initialize()
        {
            if (!_initialized)
            {
                //We overload 'pushState' because on some browsers 'locationchange' is not properly triggered.
                //However, not on 'replaceState' because we do not want to reload the page.
                Script.Write(
                    @"
    window.history.pushState = ( f => function pushState(){
        var ret = f.apply(this, arguments);
        window.dispatchEvent(new Event('pushstate'));
        window.dispatchEvent(new Event('locationchange'));
        return ret;
    })(window.history.pushState);

    window.history.replaceState = ( f => function replaceState(){
        var ret = f.apply(this, arguments);
        window.dispatchEvent(new Event('replacestate'));
        return ret;
    })(window.history.replaceState);

    window.addEventListener('popstate',()=>{
        window.dispatchEvent(new Event('locationchange'))
    });
");
                window.addEventListener("locationchange", (Action<Event>) (_ => LocationChanged(allowCallbackEvenIfLocationUnchanged: false))); // By default we'll ignore any events where the hash value hasn't actually changed
            }
            _initialized = true;
        }

        private static          bool                                   _initialized                   = false;
        private static readonly Dictionary<string, Action<Parameters>> _registedRoutesMappedToActions = new Dictionary<string, Action<Parameters>>();
        private static readonly Dictionary<string, string>             _paths                         = new Dictionary<string, string>();
        private static          List<Route>                            _routesToTryMatchingOnLocationChanged;

        public static void Push(string path)
        {
            if (AlreadyThere(path))
            {
                // Nothing to do
                return;
            }

            if (_currentState is null)
            {
                _currentState = new State(fullPath: path);
            }
            else
            {
                _currentState = _currentState.WithFullPath(path);
            }

            window.history.pushState(null, "", path);
        }

        public static void Replace(string path)
        {
            if (AlreadyThere(path))
            {
                // Nothing to do
                return;
            }

            if (_currentState is null)
            {
                _currentState = new State(fullPath: path);
            }
            else
            {
                _currentState = _currentState.WithFullPath(path);
            }

            window.history.replaceState(null, "", path);
        }

        public static Parameters GetQueryParameters() => _currentState.Parameters;

        public static void SetQueryParameters(Parameters parameters)
        {
            var url = _currentState.FullPath;

            var queryStart = url.IndexOf("?");
            if (queryStart > 0)
            {
                url = url.Substring(0, queryStart);
            }
            _currentState = new State(parameters, _currentState.RouteName, _currentState.Path, url + parameters.ToQueryString());
            window.history.replaceState(null, "", _currentState.FullPath);

        }
        
        public static void ReplaceQueryParameters(Func<Parameters, Parameters> updateFn)
        {
            var newParameters = updateFn(_currentState.Parameters.Clone());

            if (newParameters.Equals(_currentState.Parameters))
            {
                // Nothing to do
                return;
            }
            SetQueryParameters(newParameters);
        }

        /// <summary>
        /// Sometimes it is desirable to forcibly rematch the current path as if it was a new location, even if it hasn't changed - depending upon how routing is configured and how views are rendererd according to those routes, this can be useful after
        /// all of the routes have been configured as the callback from the 'Refresh' method. It can also useful if you have a path that you would like to replace with another without performing a redirect that will appear in the browser history; in that
        /// case, call Replace and then this. Note: This is equivalent to calling the Navigate method and with the current window.location.hash value and specifying reload as true.
        /// </summary>
        public static void ForceMatchCurrent() => Navigate(window.location.hash, reload: true);

        /// <summary>
        /// This will navigate the User to the specified path (pushing a new entry in the navigation history stack, so the current page / URL will appear in the browser's back button history) unless the path is that which the browser is already at - this
        /// behaviour may be overridden by setting the optional <paramref name="reload"/> to true (this does not force a reload of the page, it forces a reload of the current view by firing an OnNavigated event whether the specified path is 'new' or not)
        /// </summary>
        public static void Navigate(string path, bool reload = false)
        {
            var windowLocationSaysAlreadyThere = AlreadyThere(path);
            if (reload)
            {
                ExecuteTheNavigation();
                return;
            }

            var currentStateSaysAlreadyThere = AlreadyThere(_currentState?.FullPath, path);

            if (windowLocationSaysAlreadyThere || currentStateSaysAlreadyThere)
            {
                // Nothing to do - we're already at the right point and we're not forcing a reload
                return;
            }

            ExecuteTheNavigation();

            void ExecuteTheNavigation()
            {
                if (!windowLocationSaysAlreadyThere)
                {
                    // If the window.location doesn't indicate that we're already at the desired path then update that and the "locationchange" event listener will fire off the LocationChanged method
                    window.location.href = path;
                    return;
                }

                // If the window.location DOES indicate that we're already at the desired path then changing the path isn't going to do anything, so we'll have to force the LocationChanged call ourselves
                LocationChanged(allowCallbackEvenIfLocationUnchanged: reload);
            }
        }

        private static bool AlreadyThere(string path)
        {
            return window.location.href == path || window.location.hash == path;
        }

        private static bool AlreadyThere(string candidatePath, string hashOrPath)
        {
            if (string.IsNullOrEmpty(candidatePath)) return false;

            if (candidatePath == hashOrPath) return true;

            if (hashOrPath.StartsWith("#") || candidatePath.StartsWith("#"))
            {
                var ix1 = candidatePath.IndexOf('#');
                var ix2 = hashOrPath.IndexOf('#');

                if (ix1 < 0 && ix2 < 0) return false;

                return candidatePath.Substring(ix1) == hashOrPath.Substring(ix2);
            }

            return false;
        }

        private static string LowerCasePath(string path)
        {
            var sb = new StringBuilder();
            bool inParameter = false;
            foreach (var c in path)
            {
                if (c == ':')
                {
                    inParameter = true;
                    sb.Append(':');
                }
                else if (c == '/')
                {
                    inParameter = false;
                    sb.Append('/');
                }
                else
                {
                    if (inParameter) sb.Append(c);
                    else sb.Append(char.ToLower(c));
                }
            }
            return sb.ToString();
        }

        public static void Register(string uniqueIdentifier, string path, Func<Parameters, Task> actionTask)
        {
            Register(uniqueIdentifier, path, (p) => actionTask(p).FireAndForget());
        }

        public static void Register(string uniqueIdentifier, string path, Action<Parameters> action, bool replace = false)
        {
            uniqueIdentifier = uniqueIdentifier.ToLower();

            if (_registedRoutesMappedToActions.ContainsKey(uniqueIdentifier) && !replace)
            {
                // 2020-02-12 DWR: The last thing that the Mosaik App class does is register default routes - this means that the default routes are declared after any routes custom to the current app and this means that it
                // wouldn't be possible to have custom home pages (for example).. unless we ignore any repeat calls that specify the same uniqueIdentifier. Ignoring them allows the current app to specify a "home" route and
                // for the "home" route in the DefaultRouting.Initialize to then be ignored.
                return;
            }

            var lowerCaseID = $"path-{uniqueIdentifier}";
            var upperCaseID = $"PATH-{uniqueIdentifier.ToUpper()}";

            _paths.Remove(uniqueIdentifier);
            _paths.Remove(lowerCaseID);
            _paths.Remove(upperCaseID);
            _registedRoutesMappedToActions.Remove(uniqueIdentifier);
            _registedRoutesMappedToActions.Remove(lowerCaseID);
            _registedRoutesMappedToActions.Remove(upperCaseID);

            var lowerCasePath = LowerCasePath(path);

            if (path != lowerCasePath)
            {
                _registedRoutesMappedToActions[lowerCaseID] = action;
                _paths[lowerCaseID] = lowerCasePath;
            }

            _registedRoutesMappedToActions[uniqueIdentifier] = action;
            _paths[uniqueIdentifier] = path;

            Refresh();
        }

        public static void Refresh(Action onDone = null)
        {
            if (!_initialized)
                return;

            _routesToTryMatchingOnLocationChanged = new List<Route>();
            foreach (var kv in _paths)
            {
                _routesToTryMatchingOnLocationChanged.Add(new Route(kv.Key, kv.Value, _registedRoutesMappedToActions[kv.Key]));
            }

            onDone?.Invoke();
        }

        public static bool Exists(string hashRoute)
        {
            hashRoute = hashRoute.Split(new[] {'?'}, count: 2).First();
            return _paths.Values.Contains(hashRoute);
        }

        private static void LocationChanged(bool allowCallbackEvenIfLocationUnchanged)
        {
            var currentPathFromHash = (window.location.hash ?? "");

            if (!allowCallbackEvenIfLocationUnchanged && (_currentState is object))
            {
                if (AlreadyThere(_currentState.FullPath, currentPathFromHash))
                {
                    return;
                }
            }

            //The call to AlreadyThere above expects the hash to be still in the URL, so we only remove it here
            currentPathFromHash = currentPathFromHash.TrimStart('#');

            var p = currentPathFromHash.Split(new[] {'?'}, count: 2); // Do not remove empty entries, as we need the empty entry in the array

            var hash = (p.Length == 0) ? "" : p[0];

            var par = new Dictionary<string, string>();
            var parts = hash.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var r in _routesToTryMatchingOnLocationChanged)
            {
                par.Clear();
                if (!r.IsMatch(parts, par))
                    continue;

                if (p.Length > 1)
                {
                    //TODO parse query parameters
                    var query = p[1];
                    var queryParts = query.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var qp in queryParts)
                    {
                        var qpp = qp.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);
                        if (qpp.Length == 1)
                        {
                            par[es5.decodeURIComponent(qpp[0])] = "";
                        }
                        else
                        {
                            par[es5.decodeURIComponent(qpp[0])] = es5.decodeURIComponent(qpp[1]);
                        }
                    }
                }

                var toState = new State(
                    parameters: new Parameters(par),
                    path: hash,
                    fullPath: window.location.href,
                    routeName: r.Name
                );

                if ((_beforeNavigate is null) || _beforeNavigate(toState, _currentState))
                {
                    // Allowed to navigate - do it!
                    var oldState = _currentState;
                    _currentState = toState;
                    r.Activate(toState.Parameters);
                    Navigated?.Invoke(toState, oldState);
                }
                else
                {
                    // New route was matched but onBeforeNavigate denied navigation, so revert the current URL back to the "current state" (ie. the last state that was matched before this navigation attempt)
                    if ((_currentState is object) && !string.IsNullOrEmpty(_currentState.FullPath))
                    {
                        window.location.href = _currentState.FullPath;
                    }
                }
                return;
            }

            // If we got here without any of the routes being matched then it means we couldn't match the new URL
            NotMatched?.Invoke(parts);
        }

        public sealed class State
        {
            public State(string fullPath) : this(null, null, null, fullPath) { }
            public State(Parameters parameters, string routeName, string path, string fullPath)
            {
                Parameters = parameters;
                RouteName = routeName;
                Path = path;
                FullPath = fullPath;
            }

            public Parameters Parameters { get; }
            public string     RouteName  { get; }
            public string     Path       { get; }
            public string     FullPath   { get; }

            public State WithFullPath(string fullPath) => new State(Parameters, RouteName, Path, fullPath);
        }

        private sealed class RoutePart
        {
            public RoutePart(string path)
            {
                Path = path;
                IsVariable = path.StartsWith(":");
                VariableName = IsVariable ? path.TrimStart(':') : "";
            }

            public string Path         { get; }
            public bool   IsVariable   { get; }
            public string VariableName { get; }

            public bool IsMatch(string pathPart, out string capturedVariable)
            {
                if (IsVariable)
                {
                    capturedVariable = pathPart;
                    return true;
                }
                else
                {
                    capturedVariable = null;
                    return string.Equals(pathPart, Path, StringComparison.InvariantCultureIgnoreCase);
                }
            }
        }

        private sealed class Route
        {
            private readonly RoutePart[]        _parts;
            private readonly Action<Parameters> _action;
            public Route(string name, string path, Action<Parameters> action)
            {
                Name = name;
                Path = path;

                _parts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Select(p => new RoutePart(p)).ToArray();
                _action = action;
            }

            public string Name { get; }
            public string Path { get; }

            public bool IsMatch(string[] parts, Dictionary<string, string> parameters)
            {
                if (parts.Length == _parts.Length)
                {
                    var isMatch = true;
                    for (var i = 0; i < _parts.Length; i++)
                    {
                        isMatch &= _parts[i].IsMatch(parts[i], out var variable);
                        if (isMatch && _parts[i].IsVariable)
                        {
                            parameters.Add(_parts[i].VariableName, variable);
                        }
                        if (!isMatch)
                        {
                            return false;
                        }
                    }

                    return isMatch;
                }
                else
                {
                    return false;
                }
            }

            public void Activate(Parameters parameters) => _action(parameters);
        }
    }
}