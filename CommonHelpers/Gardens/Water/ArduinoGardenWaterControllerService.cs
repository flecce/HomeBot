using System;
using System.Net.Http;

namespace CommonHelpers.Gardens.Water
{
    public class ArduinoGardenWaterControllerService : IGardenWaterControllerService
    {
        private readonly Uri _url;

        public ArduinoGardenWaterControllerService(Uri url)
        {
            _url = url;
        }

        public bool Open()
        {
            return _getUrl(_url, " ?request=OPEN");
        }

        public bool Close()
        {
            return _getUrl(_url, " ?request=CLOSE");
        }

        public bool _getUrl(Uri url, string call)
        {
            var client = new HttpClient
            {
                BaseAddress = url
            };
            try
            {
                HttpResponseMessage resp = client.GetAsync(call).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Error while copying content to a stream.")
                {
                    throw ex;
                }
            }
            return true;
        }
    }
}