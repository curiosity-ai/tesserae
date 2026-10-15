using System;
using System.Threading.Tasks;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// One tab of a <see cref="Pivot"/>, <see cref="CardPivot"/>, <see cref="SegmentedPivot"/> or
    /// <see cref="PivotSelector"/>: its id, how to create its title and content, and whether the content is kept
    /// once rendered. The closing members are only used by <see cref="Pivot"/>.
    /// </summary>
    [Transpose.Name("tss.PivotTab")]
    internal sealed class PivotTab
    {
        private readonly Func<IComponent> _titleCreator;
        private readonly Func<IComponent> _contentCreator;
        private readonly bool             _canCacheContent;
        private          HTMLElement      _content;

        public PivotTab(string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false, bool closeable = false, Action onClosed = null, Func<Task<bool>> onBeforeClose = null)
        {
            Id               = id;
            _canCacheContent = cached;
            _contentCreator  = contentCreator;
            _titleCreator    = titleCreator;
            Closeable        = closeable;
            OnClosed         = onClosed;
            OnBeforeClose    = onBeforeClose;
        }

        public string Id { get; }

        internal bool             KeepCached    => _canCacheContent;
        internal bool             Closeable     { get; }
        internal Action           OnClosed      { get; }
        internal Func<Task<bool>> OnBeforeClose { get; }

        public IComponent CreateTitle() => _titleCreator();

        public HTMLElement RenderContent()
        {
            if (_canCacheContent && _content is object)
            {
                return _content;
            }

            _content = _contentCreator().Render();
            return _content;
        }
    }
}
