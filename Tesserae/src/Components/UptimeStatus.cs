using Transpose;

namespace Tesserae
{
    /// <summary>
    /// Predefined statuses (up, down, degraded, unknown) used by <see cref="UptimeBars"/> and <see
    /// cref="UptimeCalendar"/>.
    /// </summary>
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
