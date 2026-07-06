using EMI.EmployeeManagement.BLL;
using EMI.EmployeeManagement.Common.Exceptions;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using EMI.EmployeeManagement.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace EMI.EmployeeManagement.Tests.BLL;

public class AuthBLLTests
{
    private readonly Mock<IAuthDAL> _authDal = new();
    private readonly AuthBLL _sut;

    public AuthBLLTests()
    {
        _sut = new AuthBLL(_authDal.Object, BllMockSetup.CreateNullLogger<AuthBLL>());
    }

    [Fact]
    public async Task AuthenticateAsync_ValidCredentials_ReturnsUser()
    {
        var expected = TestDataBuilder.CreateAuthUserDto();
        _authDal.Setup(x => x.AuthenticateAsync("john", "pass123")).ReturnsAsync(expected);

        var result = await _sut.AuthenticateAsync("john", "pass123");

        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task AuthenticateAsync_InvalidCredentials_ReturnsNull()
    {
        _authDal.Setup(x => x.AuthenticateAsync("john", "wrong")).ReturnsAsync((AuthUserDto?)null);

        var result = await _sut.AuthenticateAsync("john", "wrong");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_DalThrows_PropagatesException()
    {
        _authDal.Setup(x => x.AuthenticateAsync("john", "pass123"))
            .ThrowsAsync(new DaoException("db error"));

        var act = () => _sut.AuthenticateAsync("john", "pass123");

        await act.Should().ThrowAsync<DaoException>();
    }
}
