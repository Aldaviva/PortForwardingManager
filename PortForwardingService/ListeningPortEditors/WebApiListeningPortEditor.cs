#nullable enable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PortForwardingService.ListeningPortEditors {

    internal class WebApiListeningPortEditor: ListeningPortEditor {

        private readonly HttpClient httpClient = new HttpClient();

        public async Task setListeningPort(ushort listeningPort) {
            var requestBodyJsonObject = new {
                listen_port = listeningPort
            };

            var requestBody = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("json", JsonConvert.SerializeObject(requestBodyJsonObject))
            });

            HttpResponseMessage response = await httpClient.PostAsync("http://localhost:8080/api/v2/app/setPreferences", requestBody);

            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Set qBittorrent listening port to {listeningPort} using Web API.");
        }

        public ushort? getListeningPort() {
            throw new NotImplementedException();
        }

    }

}