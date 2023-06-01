using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob
{
    [ExcludeFromCodeCoverage]
    public class PublishPriceChangeWebJob
    {
        private readonly ILogger<PublishPriceChangeWebJob> _logger;

        public PublishPriceChangeWebJob(ILogger<PublishPriceChangeWebJob> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Start()
        {
            //Add code here
            Console.WriteLine(nameof(PublishPriceChangeWebJob));
        }
    }
}
