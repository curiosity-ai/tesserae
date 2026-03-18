using H5;
using static H5.Core.dom;
public class TestClosest {
    public void Test(HTMLElement el) {
        var a = el.closest("html");
        var b = el.closest(".tippy-content");
    }
}
