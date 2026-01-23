namespace MyPhotoBiz.Enums
{
    /// <summary>
    /// Defines the booking status values.
    /// </summary>
    public enum BookingStatus
    {
        Pending,        // Client submitted request, awaiting review
        Confirmed,      // Photographer accepted the booking
        Declined,       // Photographer declined the request
        Cancelled,      // Client cancelled the booking
        Completed       // Booking converted to PhotoShoot
    }
}
