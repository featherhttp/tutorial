using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main(string[] args)
    {
        var app = WebApplication.Create(args);

        app.MapGet("/api/todos", GetTodos);
        app.MapGet("/api/todos/{id}", GetTodo);
        app.MapPost("/api/todos", CreateTodo);
        app.MapPost("/api/todos/{id}", UpdateCompleted);
        app.MapDelete("/api/todos/{id}", DeleteTodo);

        await app.RunAsync();
    }

    static async Task GetTodos(HttpContext http)
    {
        using var db = new TodoDbContext();
        var todos = await db.Todos.ToListAsync();

        await http.Response.WriteJsonAsync(todos);
    }

    static async Task GetTodo(HttpContext http)
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

        await http.Response.WriteJsonAsync(todo);
    }

    static async Task CreateTodo(HttpContext http)
    {
        var todo = await http.Request.ReadJsonAsync<TodoItem>();

        using var db = new TodoDbContext();
        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        http.Response.StatusCode = 204;
    }

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
}
