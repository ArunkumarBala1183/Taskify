using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using System.Diagnostics;

namespace Task_Management.Models.ActionFilter
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        private Stopwatch _stopwatch;
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch = Stopwatch.StartNew();
            Log.Information($"{context.ActionDescriptor.DisplayName} Execution Started");
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
           _stopwatch.Stop();
            var timeExecuted = _stopwatch.Elapsed;

            Log.Information($"{context.ActionDescriptor.DisplayName} Executed at {timeExecuted.TotalMilliseconds} sec");
        }
    }
}
