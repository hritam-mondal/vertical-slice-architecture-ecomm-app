using MediatR;
using System.Reflection;

namespace eCommerce.API.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Starting request {RequestName}, {DateTimeUtc}",
                typeof(TRequest).Name,
                DateTime.UtcNow);

            Type myType = request!.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
            foreach (PropertyInfo prop in props)
            {
                object? propValue = prop.GetValue(request);
                _logger.LogDebug("{Property} : {Value}", prop.Name, propValue);
            }

            try
            {
                var response = await next();
                _logger.LogInformation(
                    "Completed request {RequestName}, {DateTimeUtc}",
                    typeof(TRequest).Name,
                    DateTime.UtcNow);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Request failure {RequestName}, {Exception}, {DateTimeUtc}",
                    typeof(TRequest).Name,
                    ex,
                    DateTime.UtcNow);

                throw;
            }
        }
    }
}
