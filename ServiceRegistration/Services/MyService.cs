using System;

//  The MyService class generates a unique ID for each instance and logs messages with the service ID, helping us track the service lifecycle.
public class MyService : IMyService
{
    private readonly int _serviceId;

    public MyService()
    {
        _serviceId = new Random().Next(100000, 999999); // Generate a random 6-digit number
    }

    public void LogCreation(string message)
    {
        Console.WriteLine($"{message} - Service ID: {_serviceId}");
    }
}