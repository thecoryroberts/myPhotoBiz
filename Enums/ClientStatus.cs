namespace MyPhotoBiz.Enums
{
    /// <summary>
    /// Represents the status of a client in the system
    /// </summary>
    public enum ClientStatus
    {
        /// <summary>
        /// Active client who can book services and receive galleries
        /// </summary>
        Active = 0,

        /// <summary>
        /// Temporarily inactive client (e.g., on hold)
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// Archived client - soft deleted, data preserved for records
        /// </summary>
        Archived = 2
    }

    /// <summary>
    /// Client category for segmentation and prioritization
    /// </summary>
    public enum ClientCategory
    {
        /// <summary>
        /// Potential client who has shown interest
        /// </summary>
        Prospect = 0,

        /// <summary>
        /// Standard client
        /// </summary>
        Regular = 1,

        /// <summary>
        /// High-value or frequent client
        /// </summary>
        VIP = 2,

        /// <summary>
        /// Corporate or business client
        /// </summary>
        Corporate = 3
    }

    /// <summary>
    /// How the client found you (referral source)
    /// </summary>
    public enum ReferralSource
    {
        Unknown = 0,
        Website = 1,
        SocialMedia = 2,
        WordOfMouth = 3,
        SearchEngine = 4,
        Advertisement = 5,
        Referral = 6,
        ReturningClient = 7,
        WeddingExpo = 8,
        Other = 9
    }

    /// <summary>
    /// Preferred contact method for the client
    /// </summary>
    public enum ContactPreference
    {
        Email = 0,
        Phone = 1,
        SMS = 2,
        Any = 3
    }
}
