namespace Application.Models;

/// <summary>
///     Represents the result of container integrity validation
/// </summary>
public class ContainerIntegrityResult
{
    public ContainerIntegrityResult(bool isValid, IEnumerable<string> issues)
    {
        IsValid = isValid;
        Issues = issues.ToList();
    }

    /// <summary>
    ///     Indicates if the container data is valid and consistent
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    ///     List of integrity issues found during validation
    /// </summary>
    public IReadOnlyList<string> Issues { get; }

    /// <summary>
    ///     True if there are no integrity issues
    /// </summary>
    public bool HasIssues => Issues.Count > 0;
}