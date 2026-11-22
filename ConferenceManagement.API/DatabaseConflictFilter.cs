using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ConferenceManagement.API;

public sealed class DatabaseConflictFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not DbUpdateException { InnerException: SqlException sql })
        {
            return;
        }

        string? message = sql.Number switch
        {
            2601 or 2627 => "Такая запись уже существует. Проверьте индекс, логин или назначенное лицо.",
            547 => "Проверьте связанные записи. Их удаление может быть запрещено, пока они используются.",
            _ => null
        };

        if (message is null)
        {
            return;
        }

        context.Result = new ConflictObjectResult(new ProblemDetails { Status = 409, Title = message });
        context.ExceptionHandled = true;
    }
}
