using H5;

namespace Tesserae
{
    [Enum(Emit.StringName)]
    public enum UptimeStatus
    {
        Operational,
        Minor,
        Major,
        Maintenance,
        None,
        Future
    }
}
