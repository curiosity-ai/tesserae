using H5;
using static H5.Core.dom;

namespace Tesserae.Components
{
    public enum ObservableComponentUsageType
    {
        NoObservable,
        BindableObservable,
        ExportingObservable
    }


    public abstract class ObservableComponentBase<T, THTML> : ComponentBase<T, THTML> where T : ComponentBase<T, THTML> where THTML : HTMLElement
    {
        private ObservableComponentUsageType _observableUsageType = ObservableComponentUsageType.NoObservable;
        
    }
}