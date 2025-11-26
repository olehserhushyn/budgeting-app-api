using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
            {
                return result.Status switch
                {
                    ResultStatus.Ok => new OkObjectResult(result.Value),
                    ResultStatus.Created => new CreatedResult(result.Location ?? string.Empty, result.Value),
                    ResultStatus.NoContent => new NoContentResult(),
                    _ => new OkObjectResult(result.Value) // Fallback for success cases
                };
            }

            // Concatenating all error messages and validation errors into a single string
            var errorMessage = result.Errors.Any()
                ? string.Join(" ", result.Errors)
                : result.ValidationErrors.Any()
                    ? string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage))
                    : "An error occurred";

            return result.Status switch
            {
                ResultStatus.NotFound => new NotFoundObjectResult(errorMessage),
                ResultStatus.Unauthorized => new UnauthorizedObjectResult(errorMessage),
                ResultStatus.Forbidden => new ObjectResult(errorMessage) { StatusCode = 403 }, // ForbidResult can't take message
                ResultStatus.Conflict => new ConflictObjectResult(errorMessage),
                ResultStatus.Invalid => new BadRequestObjectResult(new { Errors = result.ValidationErrors }),
                ResultStatus.Error or ResultStatus.CriticalError => new ObjectResult(errorMessage) { StatusCode = 500 },
                ResultStatus.Unavailable => new ObjectResult(errorMessage) { StatusCode = 503 },
                _ => new BadRequestObjectResult(errorMessage) // Default error response
            };
        }
    }
}