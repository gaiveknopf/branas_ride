using Ride.Application.UseCase;
using Ride.Infra.Database;
using Ride.Infra.Repository;

namespace Ride.Tests.Integration;

public class RideServiceTests
{
    private readonly Signup _signup;
    private readonly RequestRide _requestRide;
    private readonly AcceptRide _acceptRide;
    private readonly StartRide _startRide;
    private readonly GetRide _getRide;


    public RideServiceTests()
    {
        var connection = new PgPromiseAdapter();
        var accountDao = new AccountDaoDatabase(connection);
        var rideDao = new RideDaoDatabase(connection);
        _signup = new Signup(accountDao);
        _requestRide = new RequestRide(rideDao, accountDao);
        _acceptRide = new AcceptRide(rideDao, accountDao);
        _startRide = new StartRide(rideDao);
        _getRide = new GetRide(rideDao);
    }

    [Fact(DisplayName = "Should create a ride")]
    public async void RequestRide_WhenDataAreValid_ShouldCreateRide()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        Assert.NotNull(outputRequestRide);
    }

    [Fact(DisplayName = "Should request and consult a ride")]
    public async void RequestRide_WhenDataAreValid_ShouldRequestAndConsultRide()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.496887588317275m, -48.522234807851476m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        var outputGetRide = await _getRide.Execute(outputRequestRide["rideId"]);
        Assert.NotNull(outputGetRide);
        Assert.Equal(outputGetRide.GetStatus(), "requested");
        Assert.Equal(outputGetRide.PassengerId, inputRequestRide.PassengerId);
        Assert.Equal(outputGetRide.FromLat, inputRequestRide.From.Lat);
        Assert.Equal(outputGetRide.FromLong, inputRequestRide.From.Long);
        Assert.Equal(outputGetRide.ToLat, inputRequestRide.To.Lat);
        Assert.Equal(outputGetRide.ToLong, inputRequestRide.To.Long);
        Assert.NotNull(outputGetRide.Date);
    }

    [Fact(DisplayName = "Should request and accept a ride")]
    public async void RequestRide_WhenDataAreValid_ShouldRequestAndAcceptRide()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        InputSignUp inputSignUpDriver = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            true,
            "AAA1234"
        );
        var outputDriver = await _signup.Execute(inputSignUpDriver);
        InputAcceptRide inputAcceptRide = new(
            outputRequestRide["rideId"],
            outputDriver
        );
        await _acceptRide.Execute(inputAcceptRide);
        var outputGetRide = await _getRide.Execute(outputRequestRide["rideId"]);
        Assert.Equal(outputGetRide.GetStatus(), "accepted");
        Assert.Equal(outputGetRide.DriverId, outputDriver);
    }

    [Fact(DisplayName = "Should throw error when request a ride with invalid passenger")]
    public async void RequestRide_WhenDataAreValid_ShouldThrowErrorWhenRequestRideWithInvalidPassenger()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            true,
            "AAA9999"
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var action = async () => await _requestRide.Execute(inputRequestRide);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Account is not from a passenger", exception.Message);
    }

    [Fact(DisplayName = "Should throw error when request a ride and passenger already has an active ride")]
    public async void RequestRide_WhenDataAreValid_ShouldThrowErrorWhenRequestRideAndPassengerAlreadyHasAnActiveRide()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        await _requestRide.Execute(inputRequestRide);
        var action = async () => await _requestRide.Execute(inputRequestRide);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("This passenger already has an active ride", exception.Message);
    }

    [Fact(DisplayName = "Should throw error when accept a ride with invalid driver")]
    public async void RequestRide_WhenDataAreValid_ShouldThrowErrorWhenAcceptRideWithInvalidDriver()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        InputSignUp inputSignUpDriver = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            false,
            "AAA9999"
        );
        var outputDriver = await _signup.Execute(inputSignUpDriver);
        InputAcceptRide inputAcceptRide = new(
            outputRequestRide["rideId"],
            outputDriver
        );
        var action = async () => await _acceptRide.Execute(inputAcceptRide);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Account is not from a driver", exception.Message);
    }

    [Fact(DisplayName = "Should throw error if status is not requested when accept a ride")]
    public async void RequestRide_WhenStatusIsNotRequested_ShouldThrowError()
    {
        InputSignUp input = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var output = await _signup.Execute(input);
        InputRequestRide inputRequestRide = new(
            output,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        InputSignUp inputSignUpDriver = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            true,
            "AAA9999"
        );
        var outputDriver = await _signup.Execute(inputSignUpDriver);
        InputAcceptRide inputAcceptRide = new(
            outputRequestRide["rideId"],
            outputDriver
        );
        await _acceptRide.Execute(inputAcceptRide);
        var action = async () => await _acceptRide.Execute(inputAcceptRide);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("The ride is not requested", exception.Message);
    }

    [Fact(DisplayName = "Should not accept a ride if the driver already has another race in progress")]
    public async void RequestRide_WhenDriverAlreadyHasAnotherRaceInProgress_ShouldNotAcceptRide()
    {
        InputSignUp inputPassenger1 = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var outputPassenger1 = await _signup.Execute(inputPassenger1);
        InputRequestRide inputRequestRide1 = new(
            outputPassenger1,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        InputSignUp inputPassenger2 = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var outputPassenger2 = await _signup.Execute(inputPassenger2);
        InputRequestRide inputRequestRide2 = new(
            outputPassenger2,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.584905257808835m, -48.545022195325124m)
        );
        var outputRequestRide1 = await _requestRide.Execute(inputRequestRide1);
        var outputRequestRide2 = await _requestRide.Execute(inputRequestRide2);
        InputSignUp inputSignUpDriver = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            true,
            "AAA9999"
        );
        var outputDriver = await _signup.Execute(inputSignUpDriver);
        InputAcceptRide inputAcceptRide1 = new(
            outputRequestRide1["rideId"],
            outputDriver
        );
        await _acceptRide.Execute(inputAcceptRide1);
        InputAcceptRide inputAcceptRide2 = new(
            outputRequestRide2["rideId"],
            outputDriver
        );
        var action = async () => await _acceptRide.Execute(inputAcceptRide2);
        var exception = await Assert.ThrowsAsync<Exception>(action);
        Assert.Equal("Driver is already in another ride", exception.Message);
    }

    [Fact(DisplayName = "Should accept and start a ride")]
    public async void RequestRide_WhenDataAreValid_ShouldAcceptAndStartRide()
    {
        InputSignUp inputPassenger = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            true,
            false,
            ""
        );
        var outputPassenger = await _signup.Execute(inputPassenger);
        InputRequestRide inputRequestRide = new(
            outputPassenger,
            From: new Location(-27.584905257808835m, -48.545022195325124m),
            To: new Location(-27.496887588317275m, -48.522234807851476m)
        );
        var outputRequestRide = await _requestRide.Execute(inputRequestRide);
        InputSignUp inputSignUpDriver = new(
            "John Doe",
            $"john.doe{Guid.NewGuid().ToString()}@gmail.com",
            "95818705552",
            false,
            true,
            "AAA9999"
        );
        var outputDriver = await _signup.Execute(inputSignUpDriver);
        InputAcceptRide inputAcceptRide = new(
            outputRequestRide["rideId"],
            outputDriver
        );
        await _acceptRide.Execute(inputAcceptRide);
        InputStartRide inputStartRide = new(
            outputRequestRide["rideId"]
        );
        await _startRide.Execute(inputStartRide);
        var outputGetRide = await _getRide.Execute(outputRequestRide["rideId"]);
        Assert.Equal(outputGetRide.GetStatus(), "in_progress");
        Assert.Equal(outputGetRide.DriverId, outputDriver);
    }
}