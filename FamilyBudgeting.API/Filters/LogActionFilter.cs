using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace FamilyBudgeting.API.Filters
{
    public class LogActionFilter : IAsyncActionFilter
    {
        private readonly ILogger<LogActionFilter> _logger;

        public LogActionFilter(ILogger<LogActionFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Code to execute BEFORE the action method
            var controllerName = context.Controller.GetType().Name;
            var actionName = context.ActionDescriptor.DisplayName; // Or context.ActionDescriptor.RouteValues["action"]

            _logger.LogInformation("Action {ControllerName}.{ActionName} is starting.", controllerName, actionName);

            var stopwatch = Stopwatch.StartNew();

            // Execute the action method and subsequent filters in the pipeline
            var resultContext = await next();

            // Code to execute AFTER the action method has completed
            stopwatch.Stop();

            if (resultContext.Exception == null)
            {
                _logger.LogInformation("Action {ControllerName}.{ActionName} finished in {ElapsedMilliseconds} ms. Status Code: {StatusCode}",
                    controllerName, actionName, stopwatch.ElapsedMilliseconds, context.HttpContext.Response.StatusCode);
            }
            else
            {
                _logger.LogError(resultContext.Exception, "Action {ControllerName}.{ActionName} failed in {ElapsedMilliseconds} ms with an exception.",
                    controllerName, actionName, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
