namespace Ride.Infra.Gateway;

public sealed class MailerGateway
{
    public MailerGateway() { }

    public Task Send(string email, string subject, string message)
    {
        Console.WriteLine($"{email}, {subject}, {message}");
        return Task.CompletedTask;
    }
}