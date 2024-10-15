namespace ApiaryAdmin;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        
        var usersGroups = app.MapGroup("/api");

        usersGroups.MapGet("/users", () => { });
        usersGroups.MapGet("/users/{userId}", (string userId) => { });
        usersGroups.MapPost("/users", () => { });
        usersGroups.MapPut("/users/{userId}", (string userId) => { });
        usersGroups.MapDelete("/users/{userId}", (string userId) => { });
        
        
        
        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
    
    public record UserDto(string Id, string FirstName, string LastName, DateTime Birthday, DateTime Created);
    public record UserCreateDto(string Id, string FirstName, string LastName, DateTime Birthday, DateTime Created);
    public record UserUpdateDto(string Id, string FirstName, string LastName, DateTime Birthday, DateTime Created);
    public record UserDeleteDto(string Id);
}