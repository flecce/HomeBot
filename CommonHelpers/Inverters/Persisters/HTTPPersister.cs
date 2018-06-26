using CommonHelpers.Inverters.Intefaces;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommonHelpers.Inverters.Persisters
{
    //public class HTTPPersister : IPersister
    //{
    //    private Enums.ConverterStatus _currentValues;
    //    private readonly IConfigurationService _configService;
    //    private TcpListener _listener = null;

    //    public HTTPPersister(IConfigurationService configService)
    //    {
    //        _configService = configService;
    //    }

    //    public bool Save(Interfaces.IInverter inv, Enums.ConverterStatus value)
    //    {
    //        value = _currentValues;
    //        if (_listener == null)
    //        {
    //            _listener = new TcpListener(Convert.ToInt32(_configService.GetConfigValue("Persisters.HTTP.Port", "9999")));
    //            _listener.Start();
    //            Thread t = new Thread(delegate ()
    //            {
    //                while (true)
    //                {
    //                    try
    //                    {
    //                        TcpClient s = _listener.AcceptTcpClient();

    //                        ProductionData product = new ProductionData();
    //                        product.ProduzioneGiornaliera = 1000;
    //                        string ser = JsonConvert.SerializeObject(product);

    //                        StringBuilder data = new StringBuilder();

    //                        data.Append("HTTP/1.1 200 OK\r\n");
    //                        data.Append(String.Format("{0:ddd, dd MMM yy HH:mm:ss} GMT\r\n", DateTime.Now));

    //                        data.Append("Cache-Control: none\r\n");
    //                        data.Append("Content-Type: application/json\r\n");
    //                        data.Append(string.Format("Content-Length: {0}\r\n", ser.Length));

    //                        data.Append("\r\n");
    //                        //data.Append(string.Format("Content-Length: {0}\r\n", file.Length));
    //                        data.Append(ser);
    //                        using (NetworkStream stream = s.GetStream())
    //                        {
    //                            byte[] bt = new byte[1024];

    //                            stream.Read(bt, 0, bt.Length);

    //                            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data.ToString());
    //                            stream.Write(msg, 0, msg.Length);
    //                        }

    //                        s.Close();
    //                    }
    //                    catch
    //                    {
    //                    }
    //                    Thread.Sleep(1000);
    //                }
    //            });
    //            t.Start();
    //        }
    //        return true;
    //    }

    //    private class ProductionData
    //    {
    //        public long ProduzioneGiornaliera { get; set; }
    //    }
    //}
}