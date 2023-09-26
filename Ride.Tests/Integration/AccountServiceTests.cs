using Ride.Application.UseCase;
using Ride.Infra.Database;
using Ride.Infra.Repository;

namespace Ride.Tests.Integration;

public class AccountServiceTests
{
    private readonly Signup _signup;
    private readonly GetAccount _getAccount;

    public AccountServiceTests()
    {
        var connection = new PgPromiseAdapter();
        var accountDao = new AccountDaoDatabase(connection);
        _signup = new Signup(accountDao);
        _getAccount = new GetAccount(accountDao);
    }

    [Fact(DisplayName = "Should create a passenger")]
    public async void SignUp_WhenDataAreValid_ShouldCreatePassenger()
    {
        InputSignUp input = new (
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        var account = await _getAccount.Execute(output);
        Assert.NotNull(account?.AccountId);
        Assert.Equal(input.Name, account.Name);
        Assert.Equal(input.Email, account.Email);
        Assert.Equal(input.Cpf, account.Cpf);
    }
    
    [Fact(DisplayName = "Should not create a passenger with invalid cpf")]
    public async void SingUp_WhenCpfInvalid_ShouldThrowException()
    {
        InputSignUp input = new (
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705500",
            true,
            false,
            ""
        );
        var action = async () => await _signup.Execute(input);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Invalid cpf", exception.Message);
    }
    
    [Fact(DisplayName = "Should not create a passenger with invalid name")]
    public async void SingUp_WhenNameInvalid_ShouldThrowException()
    {
        InputSignUp input = new (
            "John",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var action = async () => await _signup.Execute(input);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Invalid name", exception.Message);
    }
    
    [Fact(DisplayName = "Should not create a passenger with invalid email")]
    public async void SingUp_WhenEmailInvalid_ShouldThrowException()
    {
        InputSignUp input = new (
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var action = async () => await _signup.Execute(input);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Invalid email", exception.Message);
    }
    
    [Fact(DisplayName = "Should not create an account with existing email")]
    public async void SingUp_WhenEmailAlreadyExists_ShouldThrowException()
    {
        InputSignUp input = new (
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        await _signup.Execute(input);
        var action = async () => await _signup.Execute(input);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Account already exists", exception.Message);
    }
    
}