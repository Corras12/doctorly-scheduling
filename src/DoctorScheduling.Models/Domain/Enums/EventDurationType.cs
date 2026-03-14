namespace DoctorScheduling.Models.Domain.Enums;

/// <summary>
/// Defines the duration type for an event.
/// </summary>
public enum EventDurationType
{
    /// <summary>Standard 15-minute consultation.</summary>
    Standard = 0,

    /// <summary>Extended 30-minute consultation.</summary>
    Extended = 1
}
