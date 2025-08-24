namespace Domain.Constants;

/// <summary>
///     Contains constant values for default department names used in the parcel delivery system
/// </summary>
/// <remarks>
///     Default department names for system initialization.
///     These are temporary values used only for initial setup and can be modified through the business rules system.
/// </remarks>
public static class DefaultDepartmentNames
{
    /// <summary>
    ///     Department responsible for handling mail parcels
    /// </summary>
    public const string Mail = "Mail";

    /// <summary>
    ///     Department responsible for handling regular parcels
    /// </summary>
    public const string Regular = "Regular";

    /// <summary>
    ///     Department responsible for handling heavy parcels
    /// </summary>
    public const string Heavy = "Heavy";

    /// <summary>
    ///     Department responsible for insurance approval for high-value parcels
    /// </summary>
    public const string Insurance = "Insurance";
}