using EMI.EmployeeManagement.API.Controllers;
using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EMI.EmployeeManagement.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthBLL> _authBll = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretKeyForTesting1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();

        _sut = new AuthController(config, _authBll.Object);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        _authBll.Setup(x => x.AuthenticateAsync("user", "wrong")).ReturnsAsync((AuthUserDto?)null);

        var result = await _sut.Login(new UserLogin { Username = "user", Password = "wrong" });

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        _authBll.Setup(x => x.AuthenticateAsync("john", "pass123"))
            .ReturnsAsync(TestDataBuilder.CreateAuthUserDto());

        var result = await _sut.Login(new UserLogin { Username = "john", Password = "pass123" });

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var tokenProperty = okResult.Value!.GetType().GetProperty("Token");
        tokenProperty.Should().NotBeNull();
        tokenProperty!.GetValue(okResult.Value).As<string>().Should().NotBeNullOrEmpty();
    }
}
