using Ride.Domain;
using Ride.Application.Repository;
using Ride.Infra.Database;

namespace Ride.Infra.Repository;

public class RideDaoDatabase : IRideDao
{
    private readonly IConnection _connection;

    public RideDaoDatabase(IConnection connection)
    {
        this._connection = connection;
    }

    public async Task Save(Domain.Ride ride)
    {
        const string sql = """
                           INSERT INTO ride
                             (ride_id, 
                              passenger_id, 
                              driver_id, 
                              status, 
                              from_lat, 
                              from_long, 
                              to_lat, 
                              to_long, 
                              date)
                             VALUES (
                             @RideId::uuid, 
                             @PassengerId::uuid, 
                             @DriverId::uuid, 
                             @Status, 
                             @FromLat, 
                             @FromLong, 
                             @ToLat, 
                             @ToLong, 
                             @Date)
                           """;

        var parameters = new
        {
            ride.RideId,
            ride.PassengerId,
            ride.DriverId,
            ride.Status,
            ride.FromLat,
            ride.FromLong,
            ride.ToLat,
            ride.ToLong,
            ride.Date
        };

        await _connection.Query(sql, parameters);
    }

    public async Task Update(Domain.Ride ride)
    {
        const string sql = "UPDATE ride SET driver_id = @DriverId::uuid, status = @Status WHERE ride_id::text = @RideId";

        var parameters = new
        {
            ride.DriverId,
            ride.Status,
            ride.RideId
        };

        await _connection.Query(sql, parameters);
    }

    public async Task<Domain.Ride> GetById(string rideId)
    {
        const string sql = """
                            SELECT 
                                ride_id,
                                passenger_id,
                                COALESCE(driver_id::text, '') AS driver_id,
                                status,
                                fare,
                                distance,
                                from_lat,
                                from_long,
                                to_lat,
                                to_long,
                                date
                           FROM ride WHERE ride_id::text = @RideId
                           """;
        var parameters = new { RideId = rideId };

        var rideData = await _connection.Query(sql, parameters);
        return MapRide(rideData.Single());
    }

    public async Task<IEnumerable<Domain.Ride>> GetActiveRidesByPassengerId(string passengerId)
    {
        const string sql =
            """
            SELECT
                ride_id,
                passenger_id,
                COALESCE(driver_id::text, '') AS driver_id,
                status,
                fare,
                distance,
                from_lat,
                from_long,
                to_lat,
                to_long,
                date
            FROM ride WHERE passenger_id::text = @PassengerId AND status IN ('requested', 'accepted', 'in_progress')
            """;
        var parameters = new { PassengerId = passengerId };

        var ridesData = await _connection.Query(sql, parameters);
        return MapRides(ridesData);
    }

    public async Task<IEnumerable<Domain.Ride>> GetActiveRidesByDriverId(string driverId)
    {
        const string sql = "SELECT * FROM ride WHERE driver_id::text = @DriverId AND status IN ('accepted', 'in_progress')";
        var parameters = new { DriverId = driverId };

        var ridesData = await _connection.Query(sql, parameters);
        return MapRides(ridesData);
    }

    private Domain.Ride MapRide(dynamic rideData)
    {
        if (rideData == null) return null!;

        return Domain.Ride.Restore(
            rideData.ride_id.ToString(),
            rideData.passenger_id.ToString(),
            rideData.driver_id.ToString(),
            rideData.status,
            (decimal)rideData.from_lat,
            (decimal)rideData.from_long,
            (decimal)rideData.to_lat,
            (decimal)rideData.to_long,
            rideData.date
        );
    }

    private IEnumerable<Domain.Ride> MapRides(IEnumerable<dynamic> ridesData)
    {
        var rides = new List<Domain.Ride>();
        foreach (var rideData in ridesData)
        {
            var ride = MapRide(rideData);
            rides.Add(ride);
        }

        return rides;
    }
}