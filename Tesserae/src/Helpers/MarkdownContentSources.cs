using System;
using System.Collections.Generic;
using Transpose;

namespace Tesserae
{
    /// <summary>
    /// Where rendered Markdown may load content from: the URLs the browser fetches by itself, with no click, the
    /// moment the Markdown is on the page - an image, a <c>srcset</c>, a poster, a CSS <c>url(...)</c>, a picture in
    /// a <c>```mermaid</c> diagram, the data file or the map outlines of a <c>```flint</c> chart. For Markdown
    /// written by a model, such a URL is a way to send whatever the model put in its query string to whoever owns
    /// the host. It is independent of <see cref="MarkdownSanitization"/>, and the two compose:
    /// <see cref="MarkdownSanitization.NoLinksOrEmbeddedContent"/> removes links and embedded content altogether,
    /// this keeps them and decides which hosts they may come from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// What may load under a restricting policy: a URL on the page's own origin (a relative URL included), a
    /// <c>data:</c> or <c>blob:</c> URL, and an <c>http</c> / <c>https</c> URL whose host is listed. Anything else
    /// loses the attribute that would have fetched it, and the element and the text around it stay; a chart that
    /// would load from somewhere else is not drawn and stays the code block it was, with the reason as its tooltip.
    /// </para>
    /// <para>
    /// A host entry is a host name, compared case-insensitively (<c>images.example.com</c>). One starting with
    /// <c>*.</c> also matches every subdomain and the bare domain (<c>*.example.com</c> matches
    /// <c>a.example.com</c> and <c>example.com</c>). An entry is compared with the URL's host name only, so one
    /// written with a scheme, a port or a path (<c>https://example.com</c>, <c>example.com:8443</c>,
    /// <c>example.com/images</c>) never matches anything.
    /// </para>
    /// <para>
    /// Links are not content: an <c>&lt;a href&gt;</c> fetches nothing until somebody clicks it, and stays as it is.
    /// </para>
    /// </remarks>
    [Transpose.Name("tss.mdcs")]
    public sealed class MarkdownContentSources : IEquatable<MarkdownContentSources>
    {
        private static readonly string[] NoHosts = new string[0];

        private static object _script;

        private readonly string[] _hosts;

        private MarkdownContentSources(bool restricted, string[] hosts)
        {
            IsRestricted = restricted;
            _hosts       = hosts;
            Key          = restricted ? "o:" + string.Join(",", hosts) : "*";
        }

        /// <summary>Content loads from anywhere. The default: nothing is filtered, and nothing is looked at.</summary>
        public static MarkdownContentSources Any { get; } = new MarkdownContentSources(false, NoHosts);

        /// <summary>Content loads only from the page's own origin (relative URLs, <c>data:</c> and <c>blob:</c> URLs included).</summary>
        public static MarkdownContentSources SameOrigin { get; } = new MarkdownContentSources(true, NoHosts);

        /// <summary>Content loads from the page's own origin and from <paramref name="hosts"/> (see the remarks of <see cref="MarkdownContentSources"/> for what an entry is).</summary>
        public static MarkdownContentSources SameOriginAnd(params string[] hosts) => SameOriginAnd((IEnumerable<string>)hosts);

        /// <inheritdoc cref="SameOriginAnd(string[])"/>
        public static MarkdownContentSources SameOriginAnd(IEnumerable<string> hosts)
        {
            var entries = new List<string>();

            if (hosts != null)
            {
                foreach (var host in hosts)
                {
                    var entry = (host ?? string.Empty).Trim().ToLower();

                    if (entry.Length > 0 && !entries.Contains(entry)) entries.Add(entry);
                }
            }

            if (entries.Count == 0) return SameOrigin;

            //Sorted, so the same hosts in another order are the same policy.
            entries.Sort();
            return new MarkdownContentSources(true, entries.ToArray());
        }

        /// <summary>Whether this policy keeps anything from loading: false for <see cref="Any"/> only.</summary>
        public bool IsRestricted { get; }

        /// <summary>The host entries content may load from besides the page's own origin, lower-cased and sorted.</summary>
        public IReadOnlyList<string> Hosts => _hosts;

        /// <summary>A string that is the same for two policies exactly when they allow the same sources.</summary>
        public string Key { get; }

        /// <summary>
        /// Whether a URL the browser would fetch by itself may be loaded under this policy. For content an
        /// application renders itself from a URL it did not write (a thumbnail a tool or a model supplied).
        /// Under <see cref="Any"/> everything is allowed; otherwise a blank URL is not.
        /// </summary>
        public bool Allows(string url)
        {
            if (!IsRestricted) return true;
            if (string.IsNullOrWhiteSpace(url)) return false;

            return Script.Write<bool>("{0}.isAllowed({1}, {2})", GetScript(), url, _hosts);
        }

        /// <inheritdoc/>
        public bool Equals(MarkdownContentSources other) => !(other is null) && other.Key == Key;

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as MarkdownContentSources);

        /// <inheritdoc/>
        public override int GetHashCode() => Key.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => IsRestricted ? (_hosts.Length == 0 ? "SameOrigin" : "SameOriginAnd(" + string.Join(", ", _hosts) + ")") : "Any";

        /// <summary>Whether <paramref name="sources"/> keeps anything from loading; <c>null</c> is <see cref="Any"/>.</summary>
        internal static bool Restricts(MarkdownContentSources sources) => !(sources is null) && sources.IsRestricted;

        /// <summary>The key of <paramref name="sources"/>; <c>null</c> is <see cref="Any"/>.</summary>
        internal static string KeyOf(MarkdownContentSources sources) => sources is null ? Any.Key : sources.Key;

        /// <summary>
        /// <c>DOMPurify.sanitize</c> with this policy applied: a hook that is there for that one call only, taken out
        /// again by reference whatever the call does, so it never filters somebody else's sanitizing and never
        /// removes somebody else's hook. It runs on the inert document DOMPurify parses into, before any element
        /// exists that could fetch.
        /// </summary>
        /// <param name="html">The HTML to sanitize.</param>
        /// <param name="config">A DOMPurify configuration, or <c>null</c> for its default profile.</param>
        internal string Sanitize(string html, object config)
            => Script.Write<string>("{0}.sanitize({1}, {2}, {3})", GetScript(), html, config, _hosts);

        /// <summary>
        /// A function that sanitizes an SVG string into a document fragment under this policy, for a library that
        /// sanitizes what it draws whenever it draws it (Pele: on mount, and again on every relayout). The policy is
        /// applied inside the function, so it holds for a draw that happens later, outside any call of ours.
        /// </summary>
        internal object SvgSanitizer()
            => Script.Write<object>("{0}.svgSanitizer({1})", GetScript(), _hosts);

        /// <summary>
        /// Throws, with the reason, when the Flint chart spec <paramref name="input"/> (a parsed
        /// <c>ChartAssemblyInput</c>) would make the browser load from a source this policy does not allow. Flint has
        /// no hook on the loader Vega fetches with, so the spec is read before it is handed over, in one walk with
        /// three rules:
        /// <list type="bullet">
        /// <item>a string under a property named <c>url</c>, at any depth, has to be allowed - the data file
        /// (<c>data.url</c>) and the outlines of a map's own levels (<c>chartProperties.levels[].url</c>) are the two
        /// Flint reads, and the rule does not depend on knowing that;</item>
        /// <item>a <c>Map</c> or <c>Choropleth</c> chart loads Flint's built-in outlines from <c>vega.github.io</c>
        /// and <c>cdn.jsdelivr.net</c>, so both hosts have to be allowed;</item>
        /// <item>the tooltip draws the value it finds under the name <c>image</c> as a picture, on hover: a string
        /// under a property named <c>image</c> (a data row's column) has to be allowed, and nothing outside the
        /// data may rename a field to <c>image</c> (an encoding's <c>title</c>).</item>
        /// </list>
        /// Rows loaded from an allowed <c>data.url</c> are not seen here: a host that is listed is trusted with them.
        /// (The Flint that is bundled today refuses a <c>data.url</c> when it validates, so the first rule is ahead of
        /// it for the data file; it is what stops a map's own outlines.)
        /// A link (<c>chart_spec.source</c>) is not a fetch and is left alone.
        /// </summary>
        internal void CheckChart(object input)
        {
            if (!IsRestricted) return;

            var reason = Script.Write<string>("{0}.chartFetches({1}, {2})", GetScript(), input, _hosts);

            if (reason != null) throw new Exception(reason);
        }

        private static object GetScript()
        {
            if (_script == null)
            {
                _script = Script.Write<object>(@"(function () {
                    var ENTRY_POINT     = 'afterSanitizeAttributes';
                    var SVG_NAMESPACE   = 'http://www.w3.org/2000/svg';
                    var URL_ATTRIBUTES  = ['src', 'poster', 'background', 'data'];
                    var REFERENCES      = ['href', 'xlink:href'];
                    // A backslash can spell url( as an escape, so CSS that carries one is treated as fetching.
                    var CSS_FETCHES     = /\\|url\s*\(|image-set\s*\(|@import/i;
                    // A reference into the same document (a gradient, a marker, a clip path) fetches nothing.
                    var LOCAL_REFERENCE = /url\(\s*['""]?#[\w.:-]*['""]?\s*\)/gi;
                    // What Flint's Map and Choropleth charts load when the spec brings no outlines of its own.
                    var FLINT_BASEMAPS  = ['https://vega.github.io/vega-lite/data/us-10m.json', 'https://vega.github.io/vega-lite/data/world-110m.json', 'https://cdn.jsdelivr.net/npm/us-atlas@3/counties-albers-10m.json'];
                    var SVG_PROFILE     = { USE_PROFILES: { svg: true, svgFilters: true }, RETURN_DOM_FRAGMENT: true };

                    function cssFetches(css) {
                        return CSS_FETCHES.test(String(css).replace(LOCAL_REFERENCE, ''));
                    }

                    function isAllowed(value, hosts) {
                        var parsed;
                        // Text that is not a URL is not loaded: the URL constructor throws for it.
                        try { parsed = new URL(String(value).trim(), document.baseURI); } catch (e) { return false; }
                        var scheme = parsed.protocol;
                        if (scheme === 'data:' || scheme === 'blob:') return true;
                        if (scheme !== 'http:' && scheme !== 'https:') return false;
                        if (parsed.origin === window.location.origin) return true;
                        var host = parsed.hostname.toLowerCase();
                        for (var i = 0; i < hosts.length; i++) {
                            var entry = hosts[i];
                            if (entry.indexOf('*.') === 0) {
                                var parent = entry.substring(2);
                                if (parent.length > 0 && (host === parent || host.endsWith('.' + parent))) return true;
                            } else if (host === entry) {
                                return true;
                            }
                        }
                        return false;
                    }

                    // The candidates of a srcset, read the way the browser reads them: a URL is a run of
                    // non-whitespace, and a comma only separates candidates after one.
                    function isSrcsetAllowed(value, hosts) {
                        var input = String(value), position = 0;
                        while (true) {
                            while (position < input.length && /[\s,]/.test(input.charAt(position))) position++;
                            if (position >= input.length) return true;
                            var start = position;
                            while (position < input.length && !/\s/.test(input.charAt(position))) position++;
                            var url = input.substring(start, position);
                            if (/,$/.test(url)) { url = url.replace(/,+$/, ''); }
                            else { while (position < input.length && input.charAt(position) !== ',') position++; }
                            if (!isAllowed(url, hosts)) return false;
                        }
                    }

                    function createHook(hosts) {
                        return function (node) {
                            if (!node || node.nodeType !== 1) return;

                            var tag = String(node.localName || '').toLowerCase();
                            var svg = node.namespaceURI === SVG_NAMESPACE;

                            // A stylesheet reaches the network through @import and url(); one that does is emptied.
                            if (tag === 'style' && cssFetches(node.textContent)) node.textContent = '';

                            for (var i = 0; i < URL_ATTRIBUTES.length; i++) {
                                var name = URL_ATTRIBUTES[i];
                                if (node.hasAttribute(name) && !isAllowed(node.getAttribute(name), hosts)) node.removeAttribute(name);
                            }

                            if (node.hasAttribute('srcset') && !isSrcsetAllowed(node.getAttribute('srcset'), hosts)) node.removeAttribute('srcset');

                            // An href loads by itself on a stylesheet link and on what SVG draws from a reference
                            // (image, use, feImage, a pattern or filter in another file); on an anchor it is a link.
                            if (tag === 'link' || (svg && tag !== 'a')) {
                                for (var j = 0; j < REFERENCES.length; j++) {
                                    var reference = REFERENCES[j];
                                    if (node.hasAttribute(reference) && !isAllowed(node.getAttribute(reference), hosts)) node.removeAttribute(reference);
                                }
                            }

                            if (node.hasAttribute('style') && cssFetches(node.getAttribute('style'))) node.removeAttribute('style');

                            // SVG presentation attributes are CSS: fill, filter, mask and clip-path take a url() too.
                            if (svg) {
                                var names = [];
                                for (var k = 0; k < node.attributes.length; k++) names.push(node.attributes[k].name);
                                for (var l = 0; l < names.length; l++) {
                                    if (REFERENCES.indexOf(names[l]) >= 0) continue;
                                    if (cssFetches(node.getAttribute(names[l]))) node.removeAttribute(names[l]);
                                }
                            }
                        };
                    }

                    function sanitize(html, config, hosts) {
                        var hook = createHook(hosts);
                        DOMPurify.addHook(ENTRY_POINT, hook);
                        try {
                            return config ? DOMPurify.sanitize(html, config) : DOMPurify.sanitize(html);
                        } finally {
                            // By reference: a hook somebody else added meanwhile must not be the one that goes.
                            DOMPurify.removeHook(ENTRY_POINT, hook);
                        }
                    }

                    function svgSanitizer(hosts) {
                        return function (svg) { return sanitize(svg, SVG_PROFILE, hosts); };
                    }

                    function describe(value) {
                        try {
                            var parsed = new URL(String(value).trim(), document.baseURI);
                            return parsed.hostname || parsed.protocol;
                        } catch (e) {
                            return 'a URL that cannot be read';
                        }
                    }

                    // Why the chart would load from a source that is not allowed, or null.
                    function chartFetches(input, hosts) {
                        var reason = null;

                        function refuse(value) {
                            reason = 'This chart may not load content from ' + describe(value) + '.';
                        }

                        function walk(value, key, inData) {
                            if (reason !== null || value === null || value === undefined) return;

                            if (typeof value === 'string') {
                                if (key === 'url' || key === 'image') {
                                    if (!isAllowed(value, hosts)) refuse(value);
                                } else if (!inData && key !== 'field' && value.trim().toLowerCase() === 'image') {
                                    reason = 'This chart may not rename a field to ""image"": its tooltip would load the field\'s values as pictures.';
                                }
                                return;
                            }

                            if (typeof value !== 'object') return;

                            if (Array.isArray(value)) {
                                for (var i = 0; i < value.length; i++) walk(value[i], key, inData);
                                return;
                            }

                            for (var name in value) {
                                if (Object.prototype.hasOwnProperty.call(value, name)) walk(value[name], name, inData || (value === input && name === 'data'));
                            }
                        }

                        walk(input, '', false);
                        if (reason !== null) return reason;

                        var type = input && input.chart_spec && input.chart_spec.chartType;
                        type = typeof type === 'string' ? type.trim().toLowerCase() : '';

                        if (type === 'map' || type === 'choropleth') {
                            for (var i = 0; i < FLINT_BASEMAPS.length; i++) {
                                if (!isAllowed(FLINT_BASEMAPS[i], hosts)) return 'A map loads its outlines from ' + describe(FLINT_BASEMAPS[i]) + ', which this chart may not load content from.';
                            }
                        }

                        return null;
                    }

                    return { isAllowed: isAllowed, sanitize: sanitize, svgSanitizer: svgSanitizer, chartFetches: chartFetches };
                })()");
            }

            return _script;
        }
    }
}
