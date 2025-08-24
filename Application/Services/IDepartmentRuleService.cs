using Application.DTOs;
using Domain.Entities;

namespace Application.Services;

/// <summary>
///     Service for determining which departments should handle parcels based on business rules
/// </summary>
public interface IDepartmentRuleService
{
    /// <summary>
    ///     Determines which departments are required to handle a specific parcel
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <returns>Collection of departments that should handle the parcel</returns>
    /// <exception cref="ArgumentException">Thrown when parcel is not found</exception>
    Task<IEnumerable<DepartmentDto>> DetermineRequiredDepartmentsAsync(Guid parcelId);

    /// <summary>
    ///     Determines which departments should handle a parcel based on its characteristics
    /// </summary>
    /// <param name="parcel">The parcel to evaluate</param>
    /// <returns>Collection of departments that should handle the parcel</returns>
    Task<List<Department>> DetermineDepartmentsAsync(Parcel parcel);

    /// <summary>
    ///     Checks if a parcel requires insurance approval based on its value
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <returns>True if parcel requires insurance approval (value > €1000), false otherwise</returns>
    /// <exception cref="ArgumentException">Thrown when parcel is not found</exception>
    Task<bool> RequiresInsuranceApprovalAsync(Guid parcelId);

    /// <summary>
    ///     Gets departments that should handle parcels based on weight
    /// </summary>
    /// <param name="weight">Weight of the parcel in kilograms</param>
    /// <returns>Collection of departments for the specified weight range</returns>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByWeightAsync(decimal weight);

    /// <summary>
    ///     Gets departments that should handle parcels based on value (insurance requirements)
    /// </summary>
    /// <param name="value">Monetary value of the parcel</param>
    /// <returns>Collection of departments for the specified value range</returns>
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByValueAsync(decimal value);
}