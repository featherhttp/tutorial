# FeatherHttp Tutorial

In this exercise, you:

* Build the backend of a TodoReact App.
* Explore the functionality of  FeatherHttp, a server-side web framework.

**What is FeatherHttp**: FeatherHttp makes it **easy** to write web apps.  

**Why FeatherHttp**: FeatherHttp is lightweight server-side framework designed to scale-up as your app grows in complexity.

## Prerequisites

Install the following:

* [.NET Core  3.1 SDK ](https://dotnet.microsoft.com/download)
* [Node.js](https://nodejs.org/en/)

## Setup

1. Install the FeatherHttp template by running the following `dotnet CLI` command in a terminal or command prompt.
    ```
    dotnet new -i FeatherHttp.Templates::0.1.67-alpha.g69b43bed72 --nuget-source https://f.feedz.io/featherhttp/framework/nuget/index.json
    ```
    The preceding command makes the `FeatherHttp` templates available to the `dotnet new` command.
1. Download the [featherhttp repository zip file](https://github.com/featherhttp/tutorial/archive/master.zip).
1. Unzip *tutorial-master.zip* and navigate to the *Tutorial* folder. The *Tutorial* folder consists of the frontend `TodoReact` app.
1. Optional: If using [Visual Studio Code](https://code.visualstudio.com/), install the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) for C# support.

## Task:  Build the backend portion using FeatherHttp
-------------------------------------------------------

The completed exercise is available in the [samples folder](https://github.com/featherhttp/tutorial/tree/master/Sample). If you have problems following the tutorial, refer to the completed sample.

### Run the frontend app

Navigate to the *Tutorial\TodoReact* folder and run the following commands:

```
npm i
npm start
```

The preceding commands:

  - `npm i`: Restores packages using. Ignore the warning messages `npm i` generates.
  - `npm start` : Starts the react app and opens a browser window to the app that has no functionality. Ignore the **Proxy error**, that will be addressed later in the tutorial.

  ![image](https://user-images.githubusercontent.com/2546640/75070087-86307c80-54c0-11ea-8012-c78813f1dfd6.png)

Keep the React app running, it's needed for the backend in the steps laster in this tutorial.

## Build backend using FeatherHttp

1. Navigate to the *Tutorial* folder.
1. Run the following commands:

  ```
  dotnet new feather -n TodoApi
  cd TodoApi
  dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 3.1
  ```

  The preceding commands:
     - Creates a new FeatherHttp app.
     - Adds the NuGet `Microsoft.EntityFrameworkCore.InMemory` package required for later steps in this tutorial.

1. Open the *TodoApi* Folder in editor of your choice. <!-- what common editors beside VSC allow opening a folder? -->

### Create the database model

1. Create a file called  *TodoItem.cs* in the *TodoApi* folder with the following code:

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

   The preceding model is used for reading JSON and storing todo items into the database.

2. Create a file called *TodoDbContext.cs* with the following code:

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

    The preceding code:

     - Exposes a `Todos` property, which represents the list of todo items in the database.
     - `UseInMemoryDatabase` initializes the in memory database storage. Data is only be persisted while the app is running. Each time the app is restarted, the previous data is lost.

3. Use `dotnet watch` to run the server-side app:

    ```
    dotnet watch run
    ```

    `dotnet watch run` watches the app for source code changes. When a code change is made, the app is restarted and uses the new code.

### Expose the list of todo items

* Add the following `usings` to the top of the *Program.cs* file:

    ```
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    ```

    The preceding code imports the required namespaces so that the app compiles successfully.

* In `Program.cs`, create a method called `GetTodos` in the `Program` class:

    ```C#
    static async Task GetTodos(HttpContext http)
    {
        using var db = new TodoDbContext();
        var todos = await db.Todos.ToListAsync();

        await http.Response.WriteJsonAsync(todos);
    }
    ```

    The preceding code gets the list of todo items from the database and writes a JSON representation to the HTTP response.

* Connect `GetTodos` to the `api/todos` route by replacing the code in `Main` with the following:

    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);

        await app.RunAsync();
    }
    ```

<!-- Review, replaced HTTP with HTTPS and port -->
* Navigate to https://localhost:5001/api/todos. An empty JSON array is returned.

    <img src="https://user-images.githubusercontent.com/2546640/75116317-1a235500-5635-11ea-9a73-e6fc30639865.png" alt="empty json array" style="text-align:center" width =70% />

### Add a new todo item

* In *Program.cs*, add the `CreateTodo` to the `Program` class:

    ```C#
    static async Task CreateTodo(HttpContext http)
    {
        var todo = await http.Request.ReadJsonAsync<TodoItem>();

        using var db = new TodoDbContext();
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        http.Response.StatusCode = 204;
    }
    ```

    The preceding code reads the `TodoItem` from the incoming HTTP request as a JSON payload and adds it to the database.

* Connect `CreateTodo` to the `api/todos` route by modifying the code in `Main` to the following:

    ```C#
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);
        app.MapPost("/api/todos", CreateTodo);

        await app.RunAsync();
    }
    ```

* Navigate to the `TodoReact` app that is running on http://localhost:3000.
* Add a todo item in the input box. For example, add `Clean apartment`.
* Refresh the https://localhost:5001/api/todos page. The JSON output of the new todo item is displayed.
* The following image show two todo items on the `TodoReact` app:

  ![image](https://user-images.githubusercontent.com/2546640/75119637-bc056a80-5652-11ea-81c8-71ea13d97a3c.png)

### Change the state of todo items

* In *Program.cs*, create the `UpdateCompleted` method in the `Program` class:

    ```C#
    static async Task UpdateCompleted(HttpContext http)
    {
        if (!http.Request.RouteValues.TryGet("id", out int id))
        {
            http.Response.StatusCode = 400;
            return;
        }

        using var db = new TodoDbContext();
        var todo = await db.Todos.FindAsync(id);

        if (todo == null)
        {
            http.Response.StatusCode = 404;
            return;
        }

        var inputTodo = await http.Request.ReadJsonAsync<TodoItem>();
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();

        http.Response.StatusCode = 204;
    }
    ```

  The preceding code:

    * Retrieves the `id` from the route parameter "id" and uses it to find the todo item in the database.
    * Reads the JSON payload from the incoming request.
    * Sets the `IsComplete` property and updates the todo item in the database.

* Connect `UpdateCompleted` to the `api/todos/{id}` route by replacing the code in `Main` with the following:

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

### Delete a todo item

* In *Program.cs*, create a `DeleteTodo` method in the `Program` class:

    ```C#
    static async Task DeleteTodo(HttpContext http)
    {
        if (!http.Request.RouteValues.TryGet("id", out int id))
        {
            http.Response.StatusCode = 400;
            return;
        }

        using var db = new TodoDbContext();
        var todo = await db.Todos.FindAsync(id);
        if (todo == null)
        {
            http.Response.StatusCode = 404;
            return;
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        http.Response.StatusCode = 204;
    }
    ```

    The preceding code is similar to `UpdateCompleted` but instead it removes the todo item from the database.

* Connect `DeleteTodo` to the `api/todos/{id}` route by replacing the code in `Main` with the following:

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

### Test the app

The app is fully functional.

![image](https://user-images.githubusercontent.com/2546640/75119891-08ea4080-5655-11ea-96be-adab4990ad65.png)