using System;
using System.Collections.Generic;
using Tesserae;
using static Tesserae.UI;

namespace Tesserae.Bench
{
    /// <summary>
    /// Behavioural checks for toolkit bugs that only show up in a browser, run by
    /// <c>playwright/regressions.js</c> through <c>window.__bench.regressions()</c>. Each check
    /// returns null when it passes and a message when it fails; a throw counts as a failure.
    /// </summary>
    internal static class Regressions
    {
        private static readonly (string Name, Func<string> Check)[] Checks =
        {
            ("Button.ClearIcon twice, no icon",  () => ClearIconTwice(Button("x"))),
            ("Button.ClearIcon twice, had icon", () => ClearIconTwice(Button("x").SetIcon(UIcons.Star))),
        };

        public static string[] Run()
        {
            var failures = new List<string>();

            foreach (var (name, check) in Checks)
            {
                try
                {
                    var message = check();
                    if (message != null) failures.Add($"{name}: {message}");
                }
                catch (Exception e)
                {
                    failures.Add($"{name}: threw {e.Message}");
                }
            }

            return failures.ToArray();
        }

        // ClearIcon on a button without an icon used to create and then detach an icon span, so the
        // second call threw NotFoundError from removeChild.
        private static string ClearIconTwice(Button button)
        {
            var element = button.Render();

            button.ClearIcon();
            button.ClearIcon();

            if (button.Icon != null)                   return $"Icon is '{button.Icon}', expected null";
            if (element.querySelector("i") != null)    return "an icon element is still in the button";
            if (element.querySelector("span") == null) return "the text span is gone";

            button.SetIcon(UIcons.Star);
            if (element.querySelector("i") == null)    return "SetIcon after ClearIcon did not add an icon";

            return null;
        }
    }
}
