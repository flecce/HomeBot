using CommonHelpers.Inverters.Intefaces;
using System.IO;
using System.Net;

namespace CommonHelpers.Inverters.Persisters
{
    public abstract class WebPersisterBase : IPersister
    {
        protected virtual bool _doGETRequest(string URI)
        {
            string v = "";
            return _doGETRequest(URI, ref v);
        }

        protected virtual bool _doGETRequest(string URI, ref string responseValue)
        {
            WebRequest webRequest = WebRequest.Create(URI);
            try
            { // get the response
                WebResponse webResponse = webRequest.GetResponse();

                if (webResponse == null)
                { return false; }
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    string resp = sr.ReadToEnd().Trim();
                    responseValue = resp;
                    return _checkResponse(resp);
                }
            }
            catch (WebException ex)
            {
                return false;
            }
        }

        protected abstract bool _checkResponse(string resp);

        public abstract bool Save(Interfaces.IInverter inv, Enums.ConverterStatus value);
    }
}