using System.Linq.Expressions;
using MarketPOS.Application.Abstractions;
using MarketPOS.Application.Services;
using MarketPOS.Domain.Entities;
using Moq;

namespace MarketPOS.Application.Tests.Services;

/// <summary>
/// Login scenarios: success populates the session; wrong password and
/// inactive users fail with the same non-enumerable message.
/// </summary>
public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Employee>> _employees = new();
    private readonly Mock<IRepository<Role>> _roles = new();
    private readonly Mock<IRepository<AuditLog>> _auditLogs = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly UserSession _session = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWork.Setup(u => u.Repository<Employee>()).Returns(_employees.Object);
        _unitOfWork.Setup(u => u.Repository<Role>()).Returns(_roles.Object);
        _unitOfWork.Setup(u => u.Repository<AuditLog>()).Returns(_auditLogs.Object);
        _service = new AuthService(_unitOfWork.Object, _hasher.Object, _session);
    }

    private static Employee CreateEmployee(bool isActive = true) => new()
    {
        Id = 7,
        FullName = "Test Kassir",
        Username = "kassir1",
        PasswordHash = "hashed",
        RoleId = 3,
        IsActive = isActive
    };

    private void SetupEmployeeLookup(Employee? employee) =>
        _employees
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

    [Fact]
    public async Task Login_WithValidCredentials_SucceedsAndPopulatesSession()
    {
        var employee = CreateEmployee();
        SetupEmployeeLookup(employee);
        _hasher.Setup(h => h.Verify("düzgün-şifrə", "hashed")).Returns(true);
        _roles.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 3, Name = Role.Cashier });

        var result = await _service.LoginAsync("kassir1", "düzgün-şifrə");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Employee);
        Assert.Equal(Role.Cashier, result.Employee!.RoleName);
        Assert.Equal("kassir1", _session.Current?.Username);
        _auditLogs.Verify(a => a.AddAsync(
            It.Is<AuditLog>(log => log.Action == "Login" && log.EmployeeId == 7),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithWrongPassword_FailsWithoutSession()
    {
        var employee = CreateEmployee();
        SetupEmployeeLookup(employee);
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await _service.LoginAsync("kassir1", "yanlış-şifrə");

        Assert.False(result.Succeeded);
        Assert.Null(_session.Current);
        _auditLogs.Verify(a => a.AddAsync(
            It.Is<AuditLog>(log => log.Action == "LoginFailed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInactiveEmployee_Fails()
    {
        SetupEmployeeLookup(CreateEmployee(isActive: false));
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var result = await _service.LoginAsync("kassir1", "düzgün-şifrə");

        Assert.False(result.Succeeded);
        Assert.Null(_session.Current);
    }

    [Fact]
    public async Task Login_WithUnknownUser_FailsWithSameMessageAsWrongPassword()
    {
        SetupEmployeeLookup(null);

        var unknownUser = await _service.LoginAsync("yoxdur", "hər-hansı");

        SetupEmployeeLookup(CreateEmployee());
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var wrongPassword = await _service.LoginAsync("kassir1", "yanlış");

        Assert.False(unknownUser.Succeeded);
        Assert.Equal(wrongPassword.FailureMessage, unknownUser.FailureMessage);
    }
}
