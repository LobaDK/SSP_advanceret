using System;
using Microsoft.EntityFrameworkCore;
using SSP_app.Data;
using TodoApp.Data;

namespace SSP_app.Codes;

public class ToDoHandler(TodoDbContext context, HashingHandler hashingHandler)
{
    private readonly TodoDbContext _context = context;
    private readonly HashingHandler _hashingHandler = hashingHandler;

    public async Task<bool> HasEnteredCPRNumberAsync(ApplicationUser user)
    {
        return await _context.Cprs.Where(n => n.User == user.UserName && n.CprNr != null).AnyAsync();
    }

    public async Task AddCPRNumber(ApplicationUser user, string hashedCprNumber)
    {
        Cpr? cpr = await _context.Cprs.Where(n => n.User == user.UserName).FirstOrDefaultAsync();

        if (cpr == null)
        {
            cpr = new Cpr
            {
                User = user.UserName,
                CprNr = hashedCprNumber
            };
            _context.Cprs.Add(cpr);
        }
        else
        {
            cpr.CprNr = hashedCprNumber;
            _context.Cprs.Update(cpr);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<string> GetHashedCprNumberAsync(ApplicationUser user)
    {
        Cpr? cpr = await _context.Cprs.Where(n => n.User == user.UserName).FirstOrDefaultAsync();

        if (cpr != null)
        {
            return cpr.CprNr;
        }

        return null;
    }

    public async Task<List<TodoList>> GetToDoListAsync(ApplicationUser user)
    {
        List<TodoList> toDoList = await _context.TodoLists
            .Where(t => t.User.User == user.UserName)
            .ToListAsync();

        return toDoList;
    }

    public async Task AddToDoItemAsync(ApplicationUser user, string description)
    {
        Cpr? cprUser = await _context.Cprs.Where(n => n.User == user.UserName).FirstOrDefaultAsync();

        TodoList newToDoItem = new()
        {
            Item = description,
            User = cprUser
        };

        _context.TodoLists.Add(newToDoItem);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveToDoItemAsync(ApplicationUser user, int todoId)
    {
        TodoList? toDoItem = await _context.TodoLists.Where(t => t.User.User == user.UserName && t.Id == todoId).FirstOrDefaultAsync();

        if (toDoItem != null)
        {
            _context.TodoLists.Remove(toDoItem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CheckIfCprNumberIsAlreadyRegistered(string cleartextCprNumber)
    {
        List<string> hashes = await _context.Cprs.Select(c => c.CprNr).ToListAsync();

        foreach (string hash in hashes)
        {
            if (_hashingHandler.BCryptVeryifyHash(cleartextCprNumber, hash))
            {
                return true;
            }
        }

        return false;
    }
}
