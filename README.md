# Build a Todo API using .NET Core

1. Pre-requisites .NET Core 3.1, download from http://dot.net/.
1. Install feather `dotnet new -i FeatherHttp.Templates::0.1.59-alpha.g2c306f941a --nuget-source https://f.feedz.io/davidfowl/featherhttp/nuget/index.json`
1. Install [nodejs](https://nodejs.org/en/)

## Run the client side application

1. Navigate to the `TodoReact` folder and run `npm start`.
1. The application should load but will not work (the browser network tab should show errors for the backend API).

## Create a new Project

1. Create a new application with the following command:
    ```
    dotnet new feather -n TodoApi
    ```

## Create the Database model

1. Add the `TodoItem.cs` file with the following contents:
   ```C#
   using System.Text.Json.Serialization;

    public class TodoItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isComplete")]
        public bool IsComplete { get; set; }
    }
   ```
1. Add a reference to NuGet package `Microsoft.EntityFrameworkCore.InMemory` version `3.1.1` using the following command:
    ```
    dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1.1
    ```
1. Create a file called `TodoDbContext.cs` with the following contents:
    ```C#
    using Microsoft.EntityFrameworkCore;

    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> Todos { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("Todos");
        }
    }
    ```
    This code does 2 things:
     - It exposes a `Todos` property which represents the list of todo items in the database.
     - The call to `UseInMemoryDatabase` wires up the in memory database storage. Data will only be persisted as long as the application is running.

## Expose the list of todo items

1. In `Program.cs`, create a method called `GetTodos`:

    ```C#
    static async Task GetTodos(HttpContext context)
    {
        using var db = new TodoDbContext();
        var todos = await db.Todos.ToListAsync();

        await context.Response.WriteJsonAsync(todos);
    }
    ```

    This method gets the list of todo items from the database and writes a JSON representation to the HTTP response.
1. Wire up `GetTodos` to the `api/todos` route in `Main`:
    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);

        await app.RunAsync();
    }
    ```
1. Run the application with `dotnet run`. Navigate to the URL http://localhost:5000/api/todos in the browser. It should return an empty JSON array.

## Adding a new todo item

1. In `Program.cs`, create another method called `CreateTodo`:
    ```C#
    static async Task CreateTodo(HttpContext context)
    {
        var todo = await context.Request.ReadJsonAsync<TodoItem>();

        using var db = new TodoDbContext();
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        context.Response.StatusCode = 204;
    }
    ```

    The above method reads the `TodoItem` from the incoming HTTP request and as a JSON payload and adds
    it to the database.

1. Wire up `CreateTodo` to the `api/todos` route in `Main`:
    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);
        app.MapPost("/api/todos", CreateTodo);

        await app.RunAsync();
    }
    ```
1. Navigate to the `TodoReact` application which should be running on http://localhost:3000. The application should be able to add new todo items. Also, refreshing the page should show the stored todo items.