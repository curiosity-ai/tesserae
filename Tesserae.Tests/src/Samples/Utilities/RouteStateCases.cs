using System;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae.Tests.Samples
{
    /// <summary>One line of the RouteState sample: an input, what to call on it, and what must come back.</summary>
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

        public string Execute()
        {
            return Run(new RouteLocation(Input));
        }
    }

    /// <summary>
    /// The cases for <see cref="RouteLocation"/>: every way the Mosaik front-end reads the address bar, as a hash and an expected answer.
    /// Pure - nothing navigates - so they run when the page opens. The write and router cases are in <see cref="RouteStateRunner"/>.
    /// </summary>
    internal static class RouteStateCases
    {
        // Stand-ins for the 22-character ids the app puts in its URLs
        private const string U     = "AbCdEfGhIjKlMnOpQrSt12";
        private const string U2    = "ZyXwVuTsRqPoNmLkJiHg98";
        private const string UX    = U + "X";
        private const string C     = "ChAtChAtChAtChAtChAt12";
        private const string T     = "TeAmTeAmTeAmTeAmTeAm12";
        private const string F     = "FoLdErFoLdErFoLdErFo12";
        private const string ROOT  = "4jf9cbfctmgAi9cxrVk2Te";

        private static string B(bool value)   => value ? "true" : "false";
        private static string N(string value) => value ?? "null";

        private static string TryText(RouteLocation location, string key)
        {
            var found = location.TryGet(key, out var value);

            return B(found) + ", \"" + value + "\"";
        }

        // Modelled on the admin sidebar: category bases, items under them, one route two items share
        private static readonly string[] AdminItems =
        {
            "#/manage/data/nodes", "#/manage/search", "#/manage/shell", "#/manage/shell/curio", "#/manage/build",
            "#/manage/ai", "#/manage/ai/settings", "#/manage/ai/settings", "#/manage/operate/monitoring",
        };

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
            Add(A, "A5",  "#/Spaces/App?uid=" + U,    "Path; IsOn(\"#/spaces/app\")",      l => l.Path + "; " + B(l.IsOn("#/spaces/app")), "#/Spaces/App; true");
            Add(A, "A6",  "#//manage///build",        "IsOn(\"#/manage/build\")",          l => B(l.IsOn("#/manage/build")),              "true");
            Add(A, "A7",  "#/inboxes?preview=abc",    "Path",                              l => l.Path,                                   "#/inboxes");
            Add(A, "A8",  "#/manage/home?section=ai", "Path",                              l => l.Path,                                   "#/manage/home");
            Add(A, "A9",  "/inboxes?x=1",             "Path",                              l => l.Path,                                   "#/inboxes");
            Add(A, "A10", "",                         "PathOf(\"#/node/\")",               l => RouteLocation.PathOf("#/node/"),          "#/node");
            Add(A, "A11", "",                         "PathOf(\"#/clipboard?filter=images\")", l => RouteLocation.PathOf("#/clipboard?filter=images"), "#/clipboard");

            // ------------------------------------------------------------------------------------------------
            const string Bs = "B. IsOn: the sidebar predicates";

            Add(Bs, "B1",  "#/inboxes",                      "IsOn(\"#/inboxes\")",                          l => B(l.IsOn("#/inboxes")),                                 "true");
            Add(Bs, "B2",  "#/inboxes?preview=abc",          "IsOn(\"#/inboxes\")",                          l => B(l.IsOn("#/inboxes")),                                 "true");
            Add(Bs, "B3",  "#/inboxes?x=1",                  "IsOn(\"#/inboxes\")",                          l => B(l.IsOn("#/inboxes")),                                 "true");
            Add(Bs, "B4",  "#/INBOXES/",                     "IsOn(\"#/inboxes\")",                          l => B(l.IsOn("#/inboxes")),                                 "true");
            Add(Bs, "B5",  "#/inboxes/other",                "IsOn(\"#/inboxes\")",                          l => B(l.IsOn("#/inboxes")),                                 "false");
            Add(Bs, "B6",  "#/spaces/email-archive?uid=" + U, "IsOn(\"#/inboxes\")",                         l => B(l.IsOn("#/inboxes")),                                 "false");
            Add(Bs, "B7",  "#/teams?teamuid=" + T,           "IsOn(\"#/teams\")",                            l => B(l.IsOn("#/teams")),                                   "true");
            Add(Bs, "B8",  "#/chat-ai?chatUID=" + C + "&messageUID=", "IsOn(\"#/chat-ai\")",                 l => B(l.IsOn("#/chat-ai")),                                 "true");
            Add(Bs, "B9",  "#/spaces/apps",                  "IsOn(\"#/spaces\"); IsOn(\"#/spaces/apps\")",  l => B(l.IsOn("#/spaces")) + "; " + B(l.IsOn("#/spaces/apps")), "false; true");
            Add(Bs, "B10", "#/contacts/people",              "IsOn(\"#/contacts\")",                         l => B(l.IsOn("#/contacts")),                                "false");
            Add(Bs, "B11", "#/notes/new",                    "IsOn(\"#/notes\")",                            l => B(l.IsOn("#/notes")),                                   "false");
            Add(Bs, "B12", "#/home",                         "IsOn(\"#/\")",                                 l => B(l.IsOn("#/")),                                        "false");
            Add(Bs, "B13", "",                               "IsOn(\"#/\")",                                 l => B(l.IsOn("#/")),                                        "true");

            foreach (var route in SidebarRoutes)
            {
                var own = route;

                Add(Bs, "B14", own,                    "IsOn(\"" + own + "\")", l => B(l.IsOn(own)), "true");
                Add(Bs, "B14", own + "?preview=abc",   "IsOn(\"" + own + "\")", l => B(l.IsOn(own)), "true");
            }

            // ------------------------------------------------------------------------------------------------
            const string Cs = "C. IsOn with child keys";

            Add(Cs, "C1",  "#/calendar",                          "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "true");
            Add(Cs, "C2",  "#/calendar?uid=" + U,                 "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "false");
            Add(Cs, "C3",  "#/calendar?preview=abc&uid=" + U,     "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "false");
            Add(Cs, "C4",  "#/calendar?preview=abc",              "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "true");
            Add(Cs, "C5",  "#/calendar?uidx=1",                   "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "true");
            Add(Cs, "C6",  "#/calendar?x=uid",                    "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "true");
            Add(Cs, "C7",  "#/calendar?UID=1",                    "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "true");
            Add(Cs, "C8",  "#/calendar?uid=",                     "IsOn(\"#/calendar\", \"uid\")",      l => B(l.IsOn("#/calendar", "uid")),      "false");
            Add(Cs, "C9",  "#/clipboard?filter=images",           "IsOn(\"#/clipboard\", \"filter\")",  l => B(l.IsOn("#/clipboard", "filter")),  "false");
            Add(Cs, "C10", "#/clipboard?filter=text&preview=x",   "IsOn(\"#/clipboard\", \"filter\")",  l => B(l.IsOn("#/clipboard", "filter")),  "false");
            Add(Cs, "C11", "#/clipboard?preview=x",               "IsOn(\"#/clipboard\", \"filter\")",  l => B(l.IsOn("#/clipboard", "filter")),  "true");
            Add(Cs, "C12", "#/clipboard?filter=images",           "IsOn(\"#/clipboard\")",              l => B(l.IsOn("#/clipboard")),            "true");

            // ------------------------------------------------------------------------------------------------
            const string D = "D. IsUnder";

            Add(D, "D1",  "#/manage/build/frontend",     "IsUnder(\"#/manage/build\")",                                         l => B(l.IsUnder("#/manage/build")),                                         "true");
            Add(D, "D2",  "#/manage/buildx",             "IsUnder(\"#/manage/build\")",                                         l => B(l.IsUnder("#/manage/build")),                                         "false");
            Add(D, "D3",  "#/manage/shell/curio",        "IsUnder(\"#/manage/shell\"); IsUnder(\"#/manage/shell/curio\")",      l => B(l.IsUnder("#/manage/shell")) + "; " + B(l.IsUnder("#/manage/shell/curio")), "true; true");
            Add(D, "D4",  "#/manage/operate/code",       "IsUnder(\"#/manage/operate\"); IsUnder(\"#/manage/shell\")",          l => B(l.IsUnder("#/manage/operate")) + "; " + B(l.IsUnder("#/manage/shell")),     "true; false");
            Add(D, "D5",  "",                            "IsUnder(\"#/\")",                                                     l => B(l.IsUnder("#/")),                                                     "true");
            Add(D, "D5",  "#/x",                         "IsUnder(\"#/\")",                                                     l => B(l.IsUnder("#/")),                                                     "true");
            Add(D, "D5",  "#/manage/y?z=1",              "IsUnder(\"#/\")",                                                     l => B(l.IsUnder("#/")),                                                     "true");
            Add(D, "D6",  "#/preferences?id=user",       "IsUnder(\"#/preferences\")",                                          l => B(l.IsUnder("#/preferences")),                                          "true");
            Add(D, "D7",  "#/manage",                    "IsUnder(\"#/manage\")",                                               l => B(l.IsUnder("#/manage")),                                               "true");
            Add(D, "D7",  "#/manage?x=1",                "IsUnder(\"#/manage\")",                                               l => B(l.IsUnder("#/manage")),                                               "true");
            Add(D, "D7",  "#/manage/data",               "IsUnder(\"#/manage\")",                                               l => B(l.IsUnder("#/manage")),                                               "true");
            Add(D, "D8",  "#/oobe",                      "IsUnder(\"#/oobe\")",                                                 l => B(l.IsUnder("#/oobe")),                                                 "true");
            Add(D, "D8",  "#/manage/initial-setup",      "IsUnder(\"#/manage/initial-setup\")",                                 l => B(l.IsUnder("#/manage/initial-setup")),                                 "true");
            Add(D, "D9",  "#/manage/home?section=ai",    "IsUnder(\"#/manage/home\")",                                          l => B(l.IsUnder("#/manage/home")),                                          "true");
            Add(D, "D10", "#/node/" + U,                 "IsUnder(\"#/node/\")",                                                l => B(l.IsUnder("#/node/")),                                                "true");
            Add(D, "D10", "#/node",                      "IsUnder(\"#/node/\")",                                                l => B(l.IsUnder("#/node/")),                                                "true");
            Add(D, "D11", "#/empty?reload=true",         "IsOn(\"#/empty\")",                                                   l => B(l.IsOn("#/empty")),                                                   "true");
            Add(D, "D12", "#/spaces/apps",               "IsUnder(\"#/spaces\")",                                               l => B(l.IsUnder("#/spaces")),                                               "true");
            Add(D, "D13", "#/spacesx",                   "IsUnder(\"#/spaces\")",                                               l => B(l.IsUnder("#/spaces")),                                               "false");

            // ------------------------------------------------------------------------------------------------
            const string E = "E. Matches";

            Add(E, "E1",  "#/preferences?id=user",                          "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "true");
            Add(E, "E2",  "#/preferences?id=user-search-settings",          "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "false");
            Add(E, "E3",  "#/preferences?id=ai-search-processing-status",   "Matches(\"#/preferences?id=ai-search\")",     l => B(l.Matches("#/preferences?id=ai-search")),     "false");
            Add(E, "E4",  "#/preferences?id=chat-ai-provider",              "Matches(\"#/preferences?id=chat-ai\")",       l => B(l.Matches("#/preferences?id=chat-ai")),       "false");
            Add(E, "E5",  "#/preferences?id=user&preview=x",                "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "true");
            Add(E, "E6",  "#/preferences?preview=x&id=user",                "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "true");
            Add(E, "E7",  "#/preferences",                                  "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "false");
            Add(E, "E8",  "#/preferences?id=User",                          "Matches(\"#/preferences?id=user\")",          l => B(l.Matches("#/preferences?id=user")),          "false");
            Add(E, "E9",  "#/clipboard?filter=images&preview=x",            "Matches(\"#/clipboard?filter=images\")",      l => B(l.Matches("#/clipboard?filter=images")),      "true");
            Add(E, "E10", "#/clipboard?filter=text",                        "Matches(\"#/clipboard?filter=images\")",      l => B(l.Matches("#/clipboard?filter=images")),      "false");
            Add(E, "E11", "#/spaces/app?uid=" + U + "&folder=" + F,         "Matches(\"#/spaces/app?uid=U\")",             l => B(l.Matches("#/spaces/app?uid=" + U)),          "true");
            Add(E, "E12", "#/spaces/app?folder=" + F + "&uid=" + U,         "Matches(\"#/spaces/app?uid=U\")",             l => B(l.Matches("#/spaces/app?uid=" + U)),          "true");
            Add(E, "E13", "#/spaces/app?uid=" + U2,                         "Matches(\"#/spaces/app?uid=U\")",             l => B(l.Matches("#/spaces/app?uid=" + U)),          "false");
            Add(E, "E14", "#/spaces/app?uid=" + UX,                         "Matches(\"#/spaces/app?uid=U\")",             l => B(l.Matches("#/spaces/app?uid=" + U)),          "false");

            foreach (var route in new[] { "#/spaces/saved-search", "#/spaces/space", "#/calendar" })
            {
                var own = route;

                Add(E, "E15", own + "?uid=" + U,  "Matches(\"" + own + "?uid=U\")", l => B(l.Matches(own + "?uid=" + U)), "true");
                Add(E, "E15", own + "?uid=" + U2, "Matches(\"" + own + "?uid=U\")", l => B(l.Matches(own + "?uid=" + U)), "false");
            }

            var computer = "#/spaces/folder?uid=" + ROOT;

            Add(E, "E16", computer,                         "Matches(ComputerRootFolder)",  l => B(l.Matches(computer)),             "true");
            Add(E, "E17", computer + "&folder=" + F,        "Matches(ComputerRootFolder)",  l => B(l.Matches(computer)),             "true");
            Add(E, "E18", computer + "&preview=x",          "Matches(ComputerRootFolder)",  l => B(l.Matches(computer)),             "true");
            Add(E, "E19", "#/spaces/folder?uid=" + U,       "Matches(ComputerRootFolder)",  l => B(l.Matches(computer)),             "false");
            Add(E, "E20", "#/spaces/folder",                "Matches(ComputerRootFolder)",  l => B(l.Matches(computer)),             "false");
            Add(E, "E21", computer,                         "Matches(\"#/spaces/folder\")", l => B(l.Matches("#/spaces/folder")),    "true");
            Add(E, "E22", "#/x?q=a%20b",                    "Matches(\"#/x?q=a b\")",       l => B(l.Matches("#/x?q=a b")),          "true");
            Add(E, "E23", "#/x?uids=a;b;c",                 "Matches(\"#/x?uids=a;b;c\")",  l => B(l.Matches("#/x?uids=a;b;c")),     "true");
            Add(E, "E24", "#/clipboard?filter=images",      "Matches(\"#/clipboard\", \"filter\")", l => B(l.Matches("#/clipboard", "filter")), "false");

            // ------------------------------------------------------------------------------------------------
            const string Fs = "F. Mentions";

            Add(Fs, "F1", "#/node/" + U,                  "Mentions(U)",     l => B(l.Mentions(U)),          "true");
            Add(Fs, "F2", "#/spaces/app?uid=" + U,        "Mentions(U)",     l => B(l.Mentions(U)),          "true");
            Add(Fs, "F3", "#/search?preview=A,B," + U,    "Mentions(U)",     l => B(l.Mentions(U)),          "true");
            Add(Fs, "F4", "#/graph?uids=A;" + U + ";B",   "Mentions(U)",     l => B(l.Mentions(U)),          "true");
            Add(Fs, "F5", "#/spaces/folder?folder=" + U,  "Mentions(U)",     l => B(l.Mentions(U)),          "true");
            Add(Fs, "F5", "#/spaces/email-archive?root=" + U, "Mentions(U)", l => B(l.Mentions(U)),          "true");
            Add(Fs, "F6", "#/node/ABC",                   "Mentions(\"abc\")", l => B(l.Mentions("abc")),    "false");
            Add(Fs, "F7", "#/node/" + U,                  "Mentions(\"\")",  l => B(l.Mentions("")),         "false");
            Add(Fs, "F7", "",                             "Mentions(\"x\")", l => B(l.Mentions("x")),        "false");

            // ------------------------------------------------------------------------------------------------
            const string G = "G. Deepest: the deepest matching item wins";

            string Deepest(RouteLocation l) => N(l.Deepest(AdminItems));

            Add(G, "G1",  "#/manage/shell/curio",                 "Deepest(items)", Deepest, "#/manage/shell/curio");
            Add(G, "G2",  "#/manage/shell",                       "Deepest(items)", Deepest, "#/manage/shell");
            Add(G, "G3",  "#/manage/build/frontend",              "Deepest(items)", Deepest, "#/manage/build");
            Add(G, "G4",  "#/manage/ai/nlp",                      "Deepest(items)", Deepest, "#/manage/ai");
            Add(G, "G5",  "#/manage/ai",                          "Deepest(items)", Deepest, "#/manage/ai");
            Add(G, "G6",  "#/manage/search?show=config/a",        "Deepest(items)", Deepest, "#/manage/search");
            Add(G, "G7",  "#/manage/data/nodes?schema=X&show=Y",  "Deepest(items)", Deepest, "#/manage/data/nodes");
            Add(G, "G8",  "#/manage/assistant",                   "Deepest(items)", Deepest, "null");
            Add(G, "G8",  "#/inboxes",                            "Deepest(items)", Deepest, "null");
            Add(G, "G9",  "#/manage/ai/settings",                 "Deepest(items); the two items sharing it", l => N(l.Deepest(AdminItems)) + "; " + AdminItems.Count(i => i == l.Deepest(AdminItems)), "#/manage/ai/settings; 2");
            Add(G, "G10", "#/manage/operate/code",                "Deepest(items)", Deepest, "null");

            // ------------------------------------------------------------------------------------------------
            const string H = "H. Query parsing";

            Add(H, "H1",  "#/x?a=1&b=2",                         "Get(a); Get(b); Query.Count",  l => l.Get("a") + "; " + l.Get("b") + "; " + l.Query.Count,   "1; 2; 2");
            Add(H, "H2",  "#/x?a",                               "TryGet(a)",                    l => TryText(l, "a"),                                         "true, \"\"");
            Add(H, "H3",  "#/x?a=",                              "TryGet(a)",                    l => TryText(l, "a"),                                         "true, \"\"");
            Add(H, "H4",  "#/x?=v",                              "Query.Count",                  l => l.Query.Count.ToString(),                                "0");
            Add(H, "H5",  "#/x?a=b=c",                           "Get(a)",                       l => N(l.Get("a")),                                           "b=c");
            Add(H, "H6",  "#/x?a=%3D%26%2C%3B",                  "Get(a)",                       l => N(l.Get("a")),                                           "=&,;");
            Add(H, "H7",  "#/x?a=x+y",                           "Get(a)",                       l => N(l.Get("a")),                                           "x+y");
            Add(H, "H8",  "#/x?a=%E2%82%AC",                     "Get(a)",                       l => N(l.Get("a")),                                           "€");
            Add(H, "H9",  "#/x?a=%",                             "Get(a); does not throw",       l => N(l.Get("a")),                                           "%");
            Add(H, "H10", "#/x?a=1&a=2",                         "Get(a)",                       l => N(l.Get("a")),                                           "2");
            Add(H, "H11", "#/x?&&a=1&",                          "Query.Count",                  l => l.Query.Count.ToString(),                                "1");
            Add(H, "H12", "#/x?",                                "Query.Count",                  l => l.Query.Count.ToString(),                                "0");
            Add(H, "H13", "#/x?UID=1",                           "Get(uid)",                     l => N(l.Get("uid")),                                         "null");
            Add(H, "H14", "#/x?a%20b=1",                         "Get(\"a b\")",                 l => N(l.Get("a b")),                                         "1");
            Add(H, "H15", "#/x?uids=a;b;c&preview=a,b,c",        "Get(uids); Get(preview)",      l => l.Get("uids") + "; " + l.Get("preview"),                 "a;b;c; a,b,c");
            Add(H, "H16", "#/x?query=" + new string('q', 2000),  "Get(query).Length",            l => l.Get("query").Length.ToString(),                        "2000");
            Add(H, "H18", "#/node/abc",                          "Get(uid); Query.Count",        l => N(l.Get("uid")) + "; " + l.Query.Count,                  "null; 0");

            foreach (var value in new[] { "C++", "100%", "a&b", "x=y", "über", "a b", "a;b,c", "€" })
            {
                var original = value;

                Add(H, "H17", "(built from " + original + ")", "With(k, v).ToQueryString(), then parse",
                    l => N(new RouteLocation("#/x" + new Parameters().With("k", original).ToQueryString()).Get("k")), original);
            }

            // ------------------------------------------------------------------------------------------------
            const string I = "I. Presence rules";

            Add(I, "I1", "#/x?show=a",               "Has(show)",                    l => B(l.Has("show")),                                             "true");
            Add(I, "I2", "#/x?show=",                "Has(show); Get(show)",         l => B(l.Has("show")) + "; \"" + l.Get("show") + "\"",            "false; \"\"");
            Add(I, "I3", "#/x?show=%20",             "Has(show)",                    l => B(l.Has("show")),                                             "false");
            Add(I, "I4", "#/x?show",                 "Has(show)",                    l => B(l.Has("show")),                                             "false");
            Add(I, "I5", "#/x",                      "Has; Get; TryGet",             l => B(l.Has("show")) + "; " + N(l.Get("show")) + "; " + B(l.TryGet("show", out _)), "false; null; false");
            Add(I, "I6", "#/manage/home?section=",   "Get(section) != null; Has",    l => B(l.Get("section") != null) + "; " + B(l.Has("section")),    "true; false");

            return cases;
        }
    }
}
