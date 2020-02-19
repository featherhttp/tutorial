# Build a Todo API using .NET Core

1. Pre-requisites .NET Core 3.1, download from http://dot.net/.
1. Install feather `dotnet new -i FeatherHttp.Templates::0.1.59-alpha.g2c306f941a --nuget-source https://f.feedz.io/davidfowl/featherhttp/nuget/index.json`
1. Install [nodejs](https://nodejs.org/en/)


## Create a new Project

1. Create a new application with the following command:
    ```
    dotnet new feather -n TodoApi
    ```
1. Run the application by navigating to `TodoApi` and running `dotnet run`. The application should be running on http://localhost:5000.

## Run the client side application

1. Navigate to the `TodoReact` folder and run `npm install`. This should restore all of the npm packages (this could take a while).
1. Now execute `npm start` to run the application.
1. The application should load but will not work (the browser network tab should show errors for the backend API).

## Create the database model

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
   The above model will be used for both JSON reading and storing todo items in the database.
   
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
1. Restart the server side application but this time we're going to use `dotnet watch`:
    ```
    dotnet watch run
    ```

    This will watch our application for source code changes and will restart the process as a result.

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

## Changing the state of todo items
1. In `Program.cs`, create another method called `UpdateTodoItem`:
    ```C#
    static async Task UpdateCompleted(HttpContext context)
    {
        if (!context.Request.RouteValues.TryGet("id", out int id))
        {
            context.Response.StatusCode = 400;
            return;
        }

        using var db = new TodoDbContext();
        var todo = await db.Todos.FindAsync(id);

        if (todo == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        var inputTodo = await context.Request.ReadJsonAsync<TodoItem>();
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();

        context.Response.StatusCode = 204;
    }
    ```

    The above logic retrives the id from the route parameter "id" and uses it to find the todo item in the database. It then reads the JSON payload from the incoming request, sets the `IsComplete` property and updates the todo item in the database.
1. Wire up `UpdateTodoItem` to the `api/todos/{id}` route in `Main`:
    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);
        app.MapPost("/api/todos", CreateTodo);
        app.MapPost("/api/todos/{id}", UpdateCompleted);

        await app.RunAsync();
    }
    ```

## Deleting a todo item

1. In `Program.cs` create another method called `DeleteTodo`:
    ```C#
    static async Task DeleteTodo(HttpContext context)
    {
        if (!context.Request.RouteValues.TryGet("id", out int id))
        {
            context.Response.StatusCode = 400;
            return;
        }

        using var db = new TodoDbContext();
        var todo = await db.Todos.FindAsync(id);
        if (todo == null)
        {
            context.Response.StatusCode = 404;
            return;
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        context.Response.StatusCode = 204;
    }
    ```

    The above logic is very similar to `UpdateTodoItem` but instead. it removes the todo item from the database after finding it.

1. Wire up `DeleteTodo` to the `api/todos/{id}` route in `Main`:
    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);
        app.MapPost("/api/todos", CreateTodo);
        app.MapPost("/api/todos/{id}", UpdateCompleted);
        app.MapDelete("/api/todos/{id}", DeleteTodo);

        await app.RunAsync();
    }
    ```

## Test the application

The application should now be fully functional. 

## Adding a method to retrive a single todo item

For Todo application, we added HTTP endpoints for creating, reading, updating and deleting todo items. Using the knowledge gained, try adding a route `api/routes/{id}` that retrives a single todo item by id from the database and writes a JSON response. The method should be called `GetTodo`.