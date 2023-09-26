namespace Ride.Tests.Unit;

public class RideTests
{
    [Fact(DisplayName = "Should create a ride")]
    public void Create_WhenDataIsValid_ShouldCreated()
    {
        var ride = Domain.Ride.Create("", 0, 0, 0, 0);
        Assert.NotNull(ride);
        Assert.Equal("requested", ride.GetStatus());
    }
    
    [Fact(DisplayName = "Should accept a ride")]
    public void Accept_WhenRideIsValid_ShouldAccept()
    {
        var ride = Domain.Ride.Create("", 0, 0, 0, 0);
        ride.Accept("");
        Assert.Equal("accepted", ride.GetStatus());
    }
    
    [Fact(DisplayName = "Should not accept a ride when status is invalid")]
    public void Accept_WhenStatusIsInvalid_ShouldThrowException()
    {
        var ride = Domain.Ride.Restore("", "", "", "",0, 0, 0, 0, DateTime.Now);
        var action = () => ride.Accept("");
        var exception = Assert.Throws<Exception>(action);
        Assert.Equal("The ride is not requested", exception.Message);
    }
    
    [Fact(DisplayName = "Should start a ride")]
    public void Start_WhenRideIsValid_ShouldStart()
    {
        var ride = Domain.Ride.Create("", 0, 0, 0, 0);
        ride.Accept("");
        ride.Start();
        Assert.Equal("in_progress", ride.GetStatus());
    }
    
    [Fact(DisplayName = "Should not start a ride when status is invalid")]
    public void Start_WhenStatusIsInvalid_ShouldThrowException()
    {
        var ride = Domain.Ride.Restore("", "", "", "",0, 0, 0, 0, DateTime.Now);
        var action = () => ride.Start();
        var exception = Assert.Throws<Exception>(action);
        Assert.Equal("The ride is not accepted", exception.Message);
    }
}