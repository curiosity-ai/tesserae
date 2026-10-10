using System;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    /// <summary>One line of the Route State sample: an input, what to call on it, and what must come back.</summary>
    internal sealed class RouteCase
    {
        public string Section  { get; }
        public string Id       { get; }
        public string Input    { get; }
        public string Call     { get; }
        public string Expected { get; }

        /// <summary>Runs the call and returns what it produced, as text. Pure cases are given the location built from <see cref="Input"/>.</summary>
        public Func<RouteLocation, string> Run { get; }

        public RouteCase(string section, string id, string input, string call, Func<RouteLocation, string> run, string expected)
        {
            Section  = section;
            Id       = id;
            Input    = input;
            Call     = call;
            Run      = run;
            Expected = expected;
        }
    }

    /// <summary>
    /// The cases for <see cref="RouteLocation"/>, the reading behind <see cref="RoutePath"/> and <see cref="RouteQuery"/>: every way the app reads the address bar, as a hash and an expected answer.
    /// Pure - nothing navigates - so they run when the page opens. The write and router cases are in <see cref="RouteStateRunner"/>.
    /// </summary>
    internal static class RouteStateCases
    {
        // Stand-ins for the 22-character ids the app puts in its URLs
        private const string U     = "AbCdEfGhIjKlMnOpQrSt12";
        private const string C     = "ChAtChAtChAtChAtChAt12";
        private const string T     = "TeAmTeAmTeAmTeAmTeAm12";

        private static string B(bool value)   => value ? "true" : "false";
        private static string N(string value) => value ?? "null";

        private static string TryText(RouteLocation location, string key)
        {
            var found = location.TryGet(key, out var value);

            return B(found) + ", \"" + value + "\"";
        }

        // The twelve routes the app's sidebar marks as selected
        private static readonly string[] SidebarRoutes =
        {
            "#/chat-ai", "#/inboxes", "#/calendar", "#/contacts", "#/clipboard", "#/notes",
            "#/spaces/apps", "#/spaces/feeds", "#/spaces/uploaded", "#/spaces", "#/users", "#/teams",
        };

        public static IReadOnlyList<RouteCase> Pure { get; } = Build();

        private static List<RouteCase> Build()
        {
            var cases = new List<RouteCase>();

            void Add(string section, string id, string input, string call, Func<RouteLocation, string> run, string expected)
            {
                cases.Add(new RouteCase(section, id, input, call, run, expected));
            }

            // ------------------------------------------------------------------------------------------------
            const string A = "A. Path and normalisation";

            Add(A, "A1",  "",                         "Path",                              l => l.Path,                                   "#/");
            Add(A, "A2",  "#",                        "Path",                              l => l.Path,                                   "#/");
            Add(A, "A3",  "#/",                       "Path",                              l => l.Path,                                   "#/");
            Add(A, "A4",  "#/spaces/",                "Path",                              l => l.Path,                                   "#/spaces");
            Add(A, "A5",  "#/Spaces/App?uid=" + U,    "Path; IsExactly(\"#/spaces/app\")",      l => l.Path + "; " + B(l.IsExactly("#/spaces/app")), "#/Spaces/App; true");
            Add(A, "A6",  "#//manage///build",        "IsExactly(\"#/manage/build\")",     l => B(l.IsExactly("#/manage/build")),         "true");
            Add(A, "A7",  "#/inboxes?preview=abc",    "Path",                              l => l.Path,                                   "#/inboxes");
            Add(A, "A8",  "#/manage/home?section=ai", "Path",                              l => l.Path,                                   "#/manage/home");
            Add(A, "A9",  "/inboxes?x=1",             "Path",                              l => l.Path,                                   "#/inboxes");
            Add(A, "A10", "#/ab/c/de/ef/?x=%&=&&", "Path; IsExactly(\"#/ab/c/de/ef\"): a malformed query leaves the path alone", l => l.Path + "; " + B(l.IsExactly("#/ab/c/de/ef")), "#/ab/c/de/ef; true");

            // ------------------------------------------------------------------------------------------------
            const string Bs = "B. IsExactly: the sidebar predicates";

            Add(Bs, "B1",  "#/inboxes",                      "IsExactly(\"#/inboxes\")",                     l => B(l.IsExactly("#/inboxes")),                            "true");
            Add(Bs, "B2",  "#/inboxes?preview=abc",          "IsExactly(\"#/inboxes\")",                     l => B(l.IsExactly("#/inboxes")),                            "true");
            Add(Bs, "B3",  "#/inboxes?x=1",                  "IsExactly(\"#/inboxes\")",                     l => B(l.IsExactly("#/inboxes")),                            "true");
            Add(Bs, "B4",  "#/INBOXES/",                     "IsExactly(\"#/inboxes\")",                     l => B(l.IsExactly("#/inboxes")),                            "true");
            Add(Bs, "B5",  "#/inboxes/other",                "IsExactly(\"#/inboxes\")",                     l => B(l.IsExactly("#/inboxes")),                            "false");
            Add(Bs, "B6",  "#/spaces/email-archive?uid=" + U, "IsExactly(\"#/inboxes\")",                    l => B(l.IsExactly("#/inboxes")),                            "false");
            Add(Bs, "B7",  "#/teams?teamuid=" + T,           "IsExactly(\"#/teams\")",                       l => B(l.IsExactly("#/teams")),                              "true");
            Add(Bs, "B8",  "#/chat-ai?chatUID=" + C + "&messageUID=", "IsExactly(\"#/chat-ai\")",            l => B(l.IsExactly("#/chat-ai")),                            "true");
            Add(Bs, "B9",  "#/spaces/apps",                  "IsExactly(\"#/spaces\"); IsExactly(\"#/spaces/apps\")", l => B(l.IsExactly("#/spaces")) + "; " + B(l.IsExactly("#/spaces/apps")), "false; true");
            Add(Bs, "B10", "#/contacts/people",              "IsExactly(\"#/contacts\")",                    l => B(l.IsExactly("#/contacts")),                           "false");
            Add(Bs, "B11", "#/notes/new",                    "IsExactly(\"#/notes\")",                       l => B(l.IsExactly("#/notes")),                              "false");
            Add(Bs, "B12", "#/home",                         "IsExactly(\"#/\")",                            l => B(l.IsExactly("#/")),                                   "false");
            Add(Bs, "B13", "",                               "IsExactly(\"#/\")",                            l => B(l.IsExactly("#/")),                                   "true");

            foreach (var route in SidebarRoutes)
            {
                var own = route;

                Add(Bs, "B14", own,                    "IsExactly(\"" + own + "\")", l => B(l.IsExactly(own)), "true");
                Add(Bs, "B14", own + "?preview=abc",   "IsExactly(\"" + own + "\")", l => B(l.IsExactly(own)), "true");
            }

            Add(Bs, "B15", "#/empty?reload=true",  "IsExactly(\"#/empty\")", l => B(l.IsExactly("#/empty")), "true");
            Add(Bs, "B16", "#/ab/c/de/ef",         "IsExactly(\"#/ab/c/de/ef\")", l => B(l.IsExactly("#/ab/c/de/ef")), "true");
            Add(Bs, "B17", "#/ab/c/de/ef",         "IsExactly(\"#/ab/c\")", l => B(l.IsExactly("#/ab/c")), "false");

            // ------------------------------------------------------------------------------------------------
            const string Cs = "C. IsExactly with a child key (path and query read separately)";

            // The calendar belongs to a connected calendar (?uid=) once the key is there, so the page is "on" only without it
            bool OnPage(RouteLocation l, string route, string childKey) => l.IsExactly(route) && !l.TryGet(childKey, out _);

            Add(Cs, "C1",  "#/calendar",                          "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "true");
            Add(Cs, "C2",  "#/calendar?uid=" + U,                 "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "false");
            Add(Cs, "C3",  "#/calendar?preview=abc&uid=" + U,     "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "false");
            Add(Cs, "C4",  "#/calendar?preview=abc",              "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "true");
            Add(Cs, "C5",  "#/calendar?uidx=1",                   "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "true");
            Add(Cs, "C6",  "#/calendar?x=uid",                    "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "true");
            Add(Cs, "C7",  "#/calendar?UID=1",                    "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "true");
            Add(Cs, "C8",  "#/calendar?uid=",                     "IsExactly(\"#/calendar\") && !TryGet(\"uid\")",     l => B(OnPage(l, "#/calendar", "uid")),      "false");
            Add(Cs, "C9",  "#/clipboard?filter=images",           "IsExactly(\"#/clipboard\") && !TryGet(\"filter\")", l => B(OnPage(l, "#/clipboard", "filter")),  "false");
            Add(Cs, "C10", "#/clipboard?filter=text&preview=x",   "IsExactly(\"#/clipboard\") && !TryGet(\"filter\")", l => B(OnPage(l, "#/clipboard", "filter")),  "false");
            Add(Cs, "C11", "#/clipboard?preview=x",               "IsExactly(\"#/clipboard\") && !TryGet(\"filter\")", l => B(OnPage(l, "#/clipboard", "filter")),  "true");
            Add(Cs, "C12", "#/clipboard?filter=images",           "IsExactly(\"#/clipboard\")",                         l => B(l.IsExactly("#/clipboard")),          "true");

            // ------------------------------------------------------------------------------------------------
            const string D = "D. IsDescendantOf";

            Add(D, "D1",  "#/manage/build/frontend",     "IsDescendantOf(\"#/manage/build\")",                                              l => B(l.IsDescendantOf("#/manage/build")),                                                "true");
            Add(D, "D2",  "#/manage/buildx",             "IsDescendantOf(\"#/manage/build\")",                                              l => B(l.IsDescendantOf("#/manage/build")),                                                "false");
            Add(D, "D3",  "#/manage/shell/curio",        "IsDescendantOf(\"#/manage/shell\"); IsDescendantOf(\"#/manage/shell/curio\")",  l => B(l.IsDescendantOf("#/manage/shell")) + "; " + B(l.IsDescendantOf("#/manage/shell/curio")), "true; false");
            Add(D, "D4",  "#/manage/operate/code",       "IsDescendantOf(\"#/manage/operate\"); IsDescendantOf(\"#/manage/shell\")",      l => B(l.IsDescendantOf("#/manage/operate")) + "; " + B(l.IsDescendantOf("#/manage/shell")),     "true; false");
            Add(D, "D5",  "",                            "IsDescendantOf(\"#/\"): the root is not below itself",                            l => B(l.IsDescendantOf("#/")),                                                            "false");
            Add(D, "D5",  "#/x",                         "IsDescendantOf(\"#/\")",                                                          l => B(l.IsDescendantOf("#/")),                                                            "true");
            Add(D, "D5",  "#/manage/y?z=1",              "IsDescendantOf(\"#/\")",                                                          l => B(l.IsDescendantOf("#/")),                                                            "true");
            Add(D, "D6",  "#/preferences?id=user",       "IsDescendantOf(\"#/preferences\"): the page itself",                              l => B(l.IsDescendantOf("#/preferences")),                                                 "false");
            Add(D, "D7",  "#/manage",                    "IsDescendantOf(\"#/manage\")",                                                    l => B(l.IsDescendantOf("#/manage")),                                                      "false");
            Add(D, "D7",  "#/manage?x=1",                "IsDescendantOf(\"#/manage\")",                                                    l => B(l.IsDescendantOf("#/manage")),                                                      "false");
            Add(D, "D7",  "#/manage/data",               "IsDescendantOf(\"#/manage\")",                                                    l => B(l.IsDescendantOf("#/manage")),                                                      "true");
            Add(D, "D8",  "#/oobe",                      "IsDescendantOf(\"#/oobe\")",                                                      l => B(l.IsDescendantOf("#/oobe")),                                                        "false");
            Add(D, "D8",  "#/manage/initial-setup",      "IsDescendantOf(\"#/manage/initial-setup\")",                                      l => B(l.IsDescendantOf("#/manage/initial-setup")),                                        "false");
            Add(D, "D9",  "#/manage/home?section=ai",    "IsDescendantOf(\"#/manage/home\")",                                               l => B(l.IsDescendantOf("#/manage/home")),                                                 "false");
            Add(D, "D10", "#/node/" + U,                 "IsDescendantOf(\"#/node/\")",                                                     l => B(l.IsDescendantOf("#/node/")),                                                       "true");
            Add(D, "D10", "#/node",                      "IsDescendantOf(\"#/node/\")",                                                     l => B(l.IsDescendantOf("#/node/")),                                                       "false");
            Add(D, "D11", "#/spaces/apps",               "IsDescendantOf(\"#/spaces\")",                                                    l => B(l.IsDescendantOf("#/spaces")),                                                      "true");
            Add(D, "D12", "#/spacesx",                   "IsDescendantOf(\"#/spaces\")",                                                    l => B(l.IsDescendantOf("#/spaces")),                                                      "false");
            Add(D, "D13", "#/ab/c/de/ef",                "IsDescendantOf(\"#/ab/c\")",                                                      l => B(l.IsDescendantOf("#/ab/c")),                                                        "true");
            Add(D, "D14", "#/ab/c/de/ef",                "IsDescendantOf(\"#/ab/c/de/ef\")",                                                l => B(l.IsDescendantOf("#/ab/c/de/ef")),                                                  "false");
            Add(D, "D15", "#/ab/cx",                     "IsDescendantOf(\"#/ab/c\"): a longer segment is not a child",                     l => B(l.IsDescendantOf("#/ab/c")),                                                        "false");
            Add(D, "D16", "#/ab/c",                      "IsDescendantOf(\"#/ab/cx\")",                                                     l => B(l.IsDescendantOf("#/ab/cx")),                                                       "false");
            Add(D, "D17", "#/AB/C/DE/EF/",               "IsDescendantOf(\"#/ab/c\"): case and a trailing / do not matter",                 l => B(l.IsDescendantOf("#/ab/c")),                                                        "true");

            // ------------------------------------------------------------------------------------------------
            const string E = "E. Query parsing";

            Add(E, "E1",  "#/x?a=1&b=2",                         "Get(a); Get(b)",               l => l.Get("a") + "; " + l.Get("b"),                          "1; 2");
            Add(E, "E2",  "#/x?a",                               "TryGet(a)",                    l => TryText(l, "a"),                                         "true, \"\"");
            Add(E, "E3",  "#/x?a=",                              "TryGet(a)",                    l => TryText(l, "a"),                                         "true, \"\"");
            Add(E, "E4",  "#/x?=v",                              "Get(\"\"): a pair with no key",  l => N(l.Get("")),                                        "null");
            Add(E, "E5",  "#/x?a=b=c",                           "Get(a)",                       l => N(l.Get("a")),                                           "b=c");
            Add(E, "E6",  "#/x?a=%3D%26%2C%3B",                  "Get(a)",                       l => N(l.Get("a")),                                           "=&,;");
            Add(E, "E7",  "#/x?a=x+y",                           "Get(a)",                       l => N(l.Get("a")),                                           "x+y");
            Add(E, "E8",  "#/x?a=%E2%82%AC",                     "Get(a)",                       l => N(l.Get("a")),                                           "€");
            Add(E, "E9",  "#/x?a=%",                             "Get(a); does not throw",       l => N(l.Get("a")),                                           "%");
            Add(E, "E10", "#/x?a=1&a=2",                         "Get(a)",                       l => N(l.Get("a")),                                           "2");
            Add(E, "E11", "#/x?&&a=1&",                          "Get(a): empty pairs are skipped", l => N(l.Get("a")),                                        "1");
            Add(E, "E12", "#/x?",                                "Get(a)",                       l => N(l.Get("a")),                                           "null");
            Add(E, "E13", "#/x?UID=1",                           "Get(uid)",                     l => N(l.Get("uid")),                                         "null");
            Add(E, "E14", "#/x?a%20b=1",                         "Get(\"a b\")",                 l => N(l.Get("a b")),                                         "1");
            Add(E, "E15", "#/x?uids=a;b;c&preview=a,b,c",        "Get(uids); Get(preview)",      l => l.Get("uids") + "; " + l.Get("preview"),                 "a;b;c; a,b,c");
            Add(E, "E16", "#/x?query=" + new string('q', 2000),  "Get(query).Length",            l => l.Get("query").Length.ToString(),                        "2000");
            Add(E, "E18", "#/node/abc",                          "Get(uid): a route variable is not a query key", l => N(l.Get("uid")),                        "null");

            foreach (var value in new[] { "C++", "100%", "a&b", "x=y", "über", "a b", "a;b,c", "€" })
            {
                var original = value;

                Add(E, "E17", "(built from " + original + ")", "With(k, v).ToQueryString(), then parse",
                    l => N(new RouteLocation("#/x" + new Parameters().With("k", original).ToQueryString()).Get("k")), original);
            }

            Add(E, "E19", "#/x",                                 "Get(show); TryGet(show)",      l => N(l.Get("show")) + "; " + B(l.TryGet("show", out _)),    "null; false");

            return cases;
        }
    }
}
