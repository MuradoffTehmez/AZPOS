using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Models;
using MarketPOS.Domain.Entities;

namespace MarketPOS.Application.Services;

/// <summary>
/// Authenticates employees against the local store. Failure messages are
/// identical for unknown users and wrong passwords to prevent user enumeration.
/// </summary>
public sealed class AuthService : IAuthService
{
    private const string InvalidCredentialsMessage = "İstifadəçi adı və ya şifrə yanlışdır.";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSession _session;

    /// <summary>Creates the service.</summary>
    /// <param name="unitOfWork">Unit of work over the local store.</param>
    /// <param name="passwordHasher">Password hash verifier.</param>
    /// <param name="session">Terminal user session to populate on success.</param>
    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IUserSession session)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _session = session;
    }

    /// <inheritdoc />
    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return LoginResult.Failed(InvalidCredentialsMessage);
        }

        var employee = await _unitOfWork.Repository<Employee>()
            .FirstOrDefaultAsync(e => e.Username == username, cancellationToken)
            .ConfigureAwait(false);

        if (employee is null || !employee.IsActive || !_passwordHasher.Verify(password, employee.PasswordHash))
        {
            await WriteAuditAsync(employee?.Id, "LoginFailed", username, cancellationToken).ConfigureAwait(false);
            return LoginResult.Failed(InvalidCredentialsMessage);
        }

        var role = await _unitOfWork.Repository<Role>()
            .GetByIdAsync(employee.RoleId, cancellationToken)
            .ConfigureAwait(false);

        var info = new EmployeeInfo(employee.Id, employee.FullName, employee.Username, role?.Name ?? string.Empty);
        _session.SignIn(info);

        await WriteAuditAsync(employee.Id, "Login", username, cancellationToken).ConfigureAwait(false);
        return LoginResult.Success(info);
    }

    private async Task WriteAuditAsync(int? employeeId, string action, string username, CancellationToken cancellationToken)
    {
        await _unitOfWork.Repository<AuditLog>().AddAsync(new AuditLog
        {
            EmployeeId = employeeId,
            Action = action,
            EntityName = nameof(Employee),
            EntityId = employeeId,
            NewValue = username,
            Timestamp = DateTime.UtcNow
        }, cancellationToken).ConfigureAwait(false);

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
