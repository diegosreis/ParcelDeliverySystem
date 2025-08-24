namespace Domain.Constants;

/// <summary>
///     Contains default values for business rules used as fallback when no custom rules are configured
/// </summary>
/// <remarks>
///     These values are used only when the dynamic business rules system has no configured rules.
///     They serve as a safety net to ensure the system can function even without custom configuration.
/// </remarks>
public static class DefaultBusinessRuleValues
{
    /// <summary>
    ///     Default minimum value threshold for insurance department requirement (in euros)
    /// </summary>
    /// <remarks>
    ///     Parcels with value greater than this amount require insurance department approval.
    ///     This is a fallback value used when no custom value rules are configured.
    /// </remarks>
    public const decimal InsuranceValueThreshold = 1000m;

    /// <summary>
    ///     Default weight threshold for mail department (in kilograms)
    /// </summary>
    public const decimal MailWeightThreshold = 1m;

    /// <summary>
    ///     Default weight threshold for regular department (in kilograms)
    /// </summary>
    public const decimal RegularWeightThreshold = 10m;
}