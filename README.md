## Tutorial

**Goal**: In this exercise, the participants will be asked to build the backend of a TodoReact App.  The user will be exploring the functionality of  FeatherHttp, a server-side framework. 

**What is FeatherHttp**: FeatherHttp makes it **easy** to write web applications.  

**Why FeatherHttp**: FeatherHttp is lightweight server-side framework designed to scale-up as your application grows in complexity. 

# Setup

1. Install [git](https://git-scm.com/downloads).
1. Install [.NET Core  3.1 SDK ](https://dotnet.microsoft.com/download)
1. Install the template using the following command:
    ```
    dotnet new -i FeatherHttp.Templates::0.1.64-alpha.g00afb18f22 --nuget-source https://f.feedz.io/featherhttp/framework/nuget/index.json
    ```

1. Install [Node.js](https://nodejs.org/en/)
1. Clone this repository and navigate to the Tutorial folder, this consists of the frontend application `TodoReact` app.
    ```bash
    git clone git@github.com:featherhttp/tutorial.git featherhttp-tutorial
    cd featherhttp-tutorial/Tutorial 
    ```

    > If using [Visual Studio Code](https://code.visualstudio.com/), install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) for C# support.

**Task**:  Build the backend portion using FeatherHttp
-------------------------------------------------------
## Tasks
###  Run the frontend application

1. Once you clone the Todo repo, navigate to the `TodoReact` folder and run the following commands 
```
\Tutorial\TodoReact> npm i 
\Tutorial\TodoReact> npm start
```
- The commands above
    - Restores packages `npm i `
    - Starts the react app `npm start`
1. The app will load but have no functionality
![image](https://user-images.githubusercontent.com/2546640/75070087-86307c80-54c0-11ea-8012-c78813f1dfd6.png)

    > Keep this React app running as we'll need it once we build the back-end in the upcoming steps

### Build backend - FeatherHttp
**Create a new project**

1. Create a new FeatherHttp application and add the necessary packages in the `TodoApi` folder

```
Tutorial>dotnet new feather -n TodoApi
Tutorial> cd TodoApi
Tutorial\TodoApi> dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1
```
   - The commands above
     - create a new FeatherHttp application `dotnet new feather -n TodoApi`
     - Adds the NuGet packages  required in the next section `dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1`

2.  Open the `Todo` Folder in VS Code

## Create the database model

1. Create a file called  `TodoItem.cs` in the TodoApi folder. Add the content below:
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
   The above model will be used for reading in JSON and storing todo items into the database.
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

1. In `Program.cs`, create a method called `GetTodos` inside of the `Program` class:

    ```C#
    static async Task GetTodos(HttpContext context)
    {
        using var db = new TodoDbContext();
        var todos = await db.Todos.ToListAsync();

        await context.Response.WriteJsonAsync(todos);
    }
    ```

    This method gets the list of todo items from the database and writes a JSON representation to the HTTP response.
1. Wire up `GetTodos` to the `api/todos` route by modifying the code in `Main` to the following:
    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);

        await app.RunAsync();
    }
    ```
1. Run the application with `dotnet run`. Navigate to the URL http://localhost:5000/api/todos in the browser. It should return an empty JSON array.

    <img src="https://user-images.githubusercontent.com/2546640/75116317-1a235500-5635-11ea-9a73-e6fc30639865.png" alt="empty json array" style="text-align:center" width =70% />

## Adding a new todo item

1. In `Program.cs`, create another method called `CreateTodo` inside of the `Program` class:
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

1. Wire up `CreateTodo` to the `api/todos` route by modifying the code in `Main` to the following:
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
![image](https://user-images.githubusercontent.com/2546640/75119637-bc056a80-5652-11ea-81c8-71ea13d97a3c.png)

## Changing the state of todo items
1. In `Program.cs`, create another method called `UpdateTodoItem` inside of the `Program` class:
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
1. Wire up `UpdateTodoItem` to the `api/todos/{id}` route by modifying the code in `Main` to the following:
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

1. In `Program.cs` create another method called `DeleteTodo` inside of the `Program` class:
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

1. Wire up `DeleteTodo` to the `api/todos/{id}` route by modifying the code in `Main` to the following:
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
![image](https://user-images.githubusercontent.com/2546640/75119891-08ea4080-5655-11ea-96be-adab4990ad65.png)
