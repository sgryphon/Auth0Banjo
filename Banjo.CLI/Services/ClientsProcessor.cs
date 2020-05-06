using System.Linq;
using System.Threading.Tasks;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi.Paging;
using Banjo.CLI.Model;
using Microsoft.Extensions.Logging;

namespace Banjo.CLI.Services
{
    public class ClientsProcessor : IResourceTypeProcessor
    {
        public ResourceType[] ResourceTypes { get; } = { ResourceType.Clients };

        private readonly ILogger<ClientsProcessor> _logger;
        private readonly ManagementApiClientFactory _managementApiClientFactory;
        private readonly IConverter<Auth0ResourceTemplate, Client> _converter;

        public ClientsProcessor(
            ILogger<ClientsProcessor> logger,
            ManagementApiClientFactory managementApiClientFactory,
            IConverter<Auth0ResourceTemplate, Client> converter)
        {
            _managementApiClientFactory = managementApiClientFactory;
            _converter = converter;
            _logger = logger;
        }

        public async Task ProcessAsync(Auth0ResourceTemplate template)
        {
            using var managementClient = await _managementApiClientFactory.CreateAsync();

            var clientTemplates = _converter.Convert(template);

            // //todo support proper pagination - how to do this where every api call is different?!
            var results = await managementClient.Clients.GetAllAsync(new GetClientsRequest() { IsGlobal = false }, new PaginationInfo());

            var matchingClient = results.FirstOrDefault(x => string.Equals(x.Name, clientTemplates.Name));
            if (matchingClient == null)
            {
                //create
                _logger.LogInformation($"Creating a new client: {clientTemplates.Name}");
                var createClientRequest = Reflectorisor.CopyMembers<Client, ClientCreateRequest>(clientTemplates);
                var createResult = await managementClient.Clients.CreateAsync(createClientRequest);
            }
            else
            {
                _logger.LogInformation($"Updating existing client: {matchingClient.ClientId} {clientTemplates.Name}");
                //update
                var updateClientRequest = Reflectorisor.CopyMembers<Client, ClientUpdateRequest>(clientTemplates);
                
                //fix illegal options
                if (clientTemplates.JwtConfiguration != null)
                {
                    //can't specify IsSecretEncoded (~ secret_encoded) on an update operation
                    clientTemplates.JwtConfiguration.IsSecretEncoded = null;
                }
                
                var updateResult = await managementClient.Clients.UpdateAsync(matchingClient.ClientId, updateClientRequest);
            }
        }
    }
}