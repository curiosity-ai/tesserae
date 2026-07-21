using System;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Thin wrapper around the <a href="https://developer.mozilla.org/docs/Web/API/Credential_Management_API">Credential
    /// Management API</a>'s <c>navigator.credentials.store(new PasswordCredential(...))</c>.
    ///
    /// <para>
    /// After a successful sign-in, call <see cref="StorePasswordAsync"/> to ask the browser's
    /// password manager to remember the credentials. The credential is stored against
    /// <c>window.location.origin</c> — including the current subdomain — rather than against the
    /// registrable domain that browser heuristics default to when no API is used. That matters when
    /// a workspace is served from <c>tenant-a.example.com</c> and you do not want the credential
    /// auto-filled on <c>tenant-b.example.com</c>.
    /// </para>
    ///
    /// <para>
    /// The API is only available on secure contexts (HTTPS / localhost) and in browsers that
    /// expose it; on unsupported environments <see cref="StorePasswordAsync"/> resolves to
    /// <c>false</c> instead of throwing.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.CredentialStore")]
    public static class CredentialStore
    {
        /// <summary>
        /// Returns true if the current browser exposes <c>navigator.credentials</c> and the
        /// <c>PasswordCredential</c> constructor.
        /// </summary>
        public static bool IsSupported =>
            Script.Write<bool>("(typeof navigator !== 'undefined' && navigator.credentials && typeof window.PasswordCredential === 'function')");

        /// <summary>
        /// Asks the browser to store a username/password credential for the current origin.
        /// </summary>
        /// <param name="username">The username or email the user signed in with.</param>
        /// <param name="password">The password the user signed in with.</param>
        /// <param name="name">Optional human-readable account label shown in the browser's password UI.</param>
        /// <param name="iconURL">Optional icon URL shown in the browser's password UI.</param>
        /// <returns>
        /// <c>true</c> if the credential was handed off to the browser (the user may still decline
        /// the save prompt); <c>false</c> if the API is unavailable or the call failed.
        /// </returns>
        public static Task<bool> StorePasswordAsync(string username, string password, string name = null, string iconURL = null)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || !IsSupported)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            // We bind the delegate to a single C# local so Transpose emits one JS variable reference per
            // substitution slot (instead of re-inlining the lambda as an anonymous function
            // literal at each call site, which is a syntax-ambiguous expression-statement).
            Action<bool> done = ok => tcs.SetResult(ok);

            Script.Write(@"
                try {
                    var __init = { id: {0}, password: {1} };
                    if ({2}) { __init.name = {2}; }
                    if ({3}) { __init.iconURL = {3}; }
                    var __cred = new window.PasswordCredential(__init);
                    navigator.credentials.store(__cred).then(
                        function () { {4}(true); },
                        function () { {4}(false); }
                    );
                } catch (__e) {
                    {4}(false);
                }
            ", username, password, name, iconURL, done);

            return tcs.Task;
        }

        /// <summary>
        /// Asks the browser if it has a previously-stored credential for the current origin and,
        /// if so, returns its <c>id</c> (typically the username) and <c>password</c>. Returns
        /// <c>(null, null)</c> when nothing was returned (user dismissed the picker, no credential
        /// stored, or the API is unsupported).
        ///
        /// <para>
        /// Pass <paramref name="mediation"/> = <c>"silent"</c> to avoid showing the account chooser
        /// UI (returns a credential only if the browser is confident there is exactly one), or
        /// <c>"optional"</c> (default) to let the browser prompt the user.
        /// </para>
        /// </summary>
        public static Task<(string id, string password)> TryGetStoredPasswordAsync(string mediation = "optional")
        {
            var tcs = new TaskCompletionSource<(string, string)>();

            if (!IsSupported)
            {
                tcs.SetResult((null, null));
                return tcs.Task;
            }

            Action<string, string> done = (id, pw) => tcs.SetResult((id, pw));

            Script.Write(@"
                try {
                    navigator.credentials.get({ password: true, mediation: {0} }).then(
                        function (__cred) {
                            if (__cred && __cred.type === 'password') {
                                {1}(__cred.id, __cred.password);
                            } else {
                                {1}(null, null);
                            }
                        },
                        function () { {1}(null, null); }
                    );
                } catch (__e) {
                    {1}(null, null);
                }
            ", mediation, done);

            return tcs.Task;
        }

        /// <summary>
        /// Hints to the browser that the user signed out, so the next call to
        /// <c>navigator.credentials.get</c> will not silently auto-sign-in. Safe to call on
        /// browsers without the API (no-op).
        /// </summary>
        public static Task PreventSilentAccessAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            if (!IsSupported)
            {
                tcs.SetResult(true);
                return tcs.Task;
            }

            Action<bool> done = _ => tcs.SetResult(true);

            Script.Write(@"
                try {
                    navigator.credentials.preventSilentAccess().then(
                        function () { {0}(true); },
                        function () { {0}(false); }
                    );
                } catch (__e) {
                    {0}(true);
                }
            ", done);

            return tcs.Task;
        }
    }
}
