using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Events;
using CommonHelpers.Inverters.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommonHelpers.Inverters.Plugins.Fimer
{
    public class FimerR25Inverter : IInverter
    {
        public event EventHandlers.DataReceivedEventHandler DataReceived;

        private byte[] fileRawData;
        private byte[] file12Data;
        private byte[] file18Data;
        private byte[] file28Data;
        private IPEndPoint _endPoint = null;
        private int _serialNumber = 0;
        private TcpClient _client;
        private NetworkStream _clientStream;

        private TransmissionState ConnectTcpIndividual()
        {
            try
            {
                _client.Connect(_endPoint);
                _clientStream = _client.GetStream();
            }
            catch (ThreadAbortException)
            {
                return TransmissionState.Canceled;
            }
            catch (SocketException)
            {
                return TransmissionState.Error;
            }

            return TransmissionState.Ok;
        }

        private TransmissionState DoReadParameter(int parameter, out uint value, int converterType)
        {
            TransmissionState? transmissionState = null;
            value = 0;
            int num = 3;
            do
            {
                if (!transmissionState.HasValue)
                {
                    Thread.Sleep(50);
                }
                transmissionState = new TransmissionState?(this.DoReadParameterIntern(parameter, out value, converterType));
            }
            while (transmissionState.Value != TransmissionState.NotConnected && transmissionState.Value != TransmissionState.Ok && --num >= 0);

            return transmissionState.Value;
        }

        private void DiscardBuffers(TcpClient tcpClient, Stream stream)
        {
            if (stream.CanWrite)
            {
                stream.Flush();
            }
            if (stream.CanRead)
            {
                byte[] buffer = new byte[256];
                while (tcpClient.Available > 0)
                {
                    stream.Read(buffer, 0, Math.Min(tcpClient.Available, 256));
                }
            }
        }

        private void WriteData(byte[] buffer)
        {
            WriteData(buffer, 0, buffer.Length);
        }

        private void WriteData(byte[] buffer, int offset, int count)
        {
            DiscardBuffers(_client, _clientStream);
            _clientStream.Write(buffer, offset, count);

            if (this.DataReceived != null)
            {
                byte[] array = new byte[count];
                Array.Copy(buffer, offset, array, 0, count);
                try
                {
                    this.DataReceived(array);
                }
                catch
                {
                }
            }
        }

        private CommonStatus ReadCommonBlock(int index)
        {
            ushort status = (ushort)(((int)this.fileRawData[index + 1] << 8) + (int)this.fileRawData[index]);
            ushort status2 = (ushort)(((int)this.fileRawData[index + 3] << 8) + (int)this.fileRawData[index + 2]);
            ushort status3 = (ushort)(((int)this.fileRawData[index + 5] << 8) + (int)this.fileRawData[index + 4]);
            int num = (int)this.fileRawData[index + 9] << 24;
            num += (int)this.fileRawData[index + 8] << 16;
            num += (int)this.fileRawData[index + 7] << 8;
            num += (int)this.fileRawData[index + 6];
            float energieTag = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[8]);
            num = (int)this.fileRawData[index + 13] << 24;
            num += (int)this.fileRawData[index + 12] << 16;
            num += (int)this.fileRawData[index + 11] << 8;
            num += (int)this.fileRawData[index + 10];
            float energieWoche = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[9]);
            num = (int)this.fileRawData[index + 17] << 24;
            num += (int)this.fileRawData[index + 16] << 16;
            num += (int)this.fileRawData[index + 15] << 8;
            num += (int)this.fileRawData[index + 14];
            float energieMonat = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[10]);
            num = (int)this.fileRawData[index + 21] << 24;
            num += (int)this.fileRawData[index + 20] << 16;
            num += (int)this.fileRawData[index + 19] << 8;
            num += (int)this.fileRawData[index + 18];
            float energieJahr = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[11]);
            num = (int)this.fileRawData[index + 25] << 24;
            num += (int)this.fileRawData[index + 24] << 16;
            num += (int)this.fileRawData[index + 23] << 8;
            num += (int)this.fileRawData[index + 22];
            float energieGesamt = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[12]);
            byte letzterFehler = this.fileRawData[index + 26];
            return new CommonStatus(status, status2, status3, energieTag, energieWoche, energieMonat, energieJahr, energieGesamt, letzterFehler);
        }

        private Constants.SP50Status ReadSP50Block(int index)
        {
            ushort dcNennleistung = (ushort)(((int)this.fileRawData[index + 1] << 8) + (int)this.fileRawData[index]);
            ushort acLeistung = (ushort)(((int)this.fileRawData[index + 3] << 8) + (int)this.fileRawData[index + 2]);
            float netzfrequenz = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 5] << 8) + (int)this.fileRawData[index + 4]), GlobalData.ParamDefs[15]);
            float udc = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 7] << 8) + (int)this.fileRawData[index + 6]), GlobalData.ParamDefs[2]);
            float uac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 9] << 8) + (int)this.fileRawData[index + 8]), GlobalData.ParamDefs[1]);
            float idc = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 11] << 8) + (int)this.fileRawData[index + 10]), GlobalData.ParamDefs[4]);
            float iac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 13] << 8) + (int)this.fileRawData[index + 12]), GlobalData.ParamDefs[3]);
            float geraetetemperatur = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 15] << 8) + (int)this.fileRawData[index + 14]), GlobalData.ParamDefs[16]);
            float netzimpedand = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 17] << 8) + (int)this.fileRawData[index + 16]), GlobalData.ParamDefs[13]);
            float fehlstrom = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 19] << 8) + (int)this.fileRawData[index + 18]), GlobalData.ParamDefs[24]);
            float einstrahlung = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 21] << 8) + (int)this.fileRawData[index + 20]), GlobalData.ParamDefs[19]);
            float modultemperatur = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 23] << 8) + (int)this.fileRawData[index + 22]), GlobalData.ParamDefs[17]);
            float sensorertrag = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 25] << 8) + (int)this.fileRawData[index + 24]), GlobalData.ParamDefs[217]);
            return new Constants.SP50Status(dcNennleistung, acLeistung, netzfrequenz, udc, uac, idc, iac, geraetetemperatur, netzimpedand, fehlstrom, einstrahlung, modultemperatur, sensorertrag);
        }

        private Constants.SP120Status ReadSP120Block(int index)
        {
            ushort dcNennleistung = (ushort)(((int)this.fileRawData[index + 1] << 8) + (int)this.fileRawData[index]);
            ushort acLeistung = (ushort)(((int)this.fileRawData[index + 3] << 8) + (int)this.fileRawData[index + 2]);
            float uac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 5] << 8) + (int)this.fileRawData[index + 4]), GlobalData.ParamDefs[355]);
            float uac2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 7] << 8) + (int)this.fileRawData[index + 6]), GlobalData.ParamDefs[356]);
            float uac3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 9] << 8) + (int)this.fileRawData[index + 8]), GlobalData.ParamDefs[357]);
            float uacPeak = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 11] << 8) + (int)this.fileRawData[index + 10]), GlobalData.ParamDefs[359]);
            float uacPeak2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 13] << 8) + (int)this.fileRawData[index + 12]), GlobalData.ParamDefs[360]);
            float uacPeak3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 15] << 8) + (int)this.fileRawData[index + 14]), GlobalData.ParamDefs[361]);
            float pac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 17] << 8) + (int)this.fileRawData[index + 16]), GlobalData.ParamDefs[351]);
            float pac2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 19] << 8) + (int)this.fileRawData[index + 18]), GlobalData.ParamDefs[352]);
            float pac3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 21] << 8) + (int)this.fileRawData[index + 20]), GlobalData.ParamDefs[353]);
            float netzfrequenz = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 23] << 8) + (int)this.fileRawData[index + 22]), GlobalData.ParamDefs[376]);
            float netzfrequenz2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 25] << 8) + (int)this.fileRawData[index + 24]), GlobalData.ParamDefs[377]);
            float netzfrequenz3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 27] << 8) + (int)this.fileRawData[index + 26]), GlobalData.ParamDefs[378]);
            float temperaturWR = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 29] << 8) + (int)this.fileRawData[index + 28]), GlobalData.ParamDefs[371]);
            float temperaturWR2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 31] << 8) + (int)this.fileRawData[index + 30]), GlobalData.ParamDefs[372]);
            float temperaturWR3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 33] << 8) + (int)this.fileRawData[index + 32]), GlobalData.ParamDefs[373]);
            float udc = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 35] << 8) + (int)this.fileRawData[index + 34]), GlobalData.ParamDefs[528]);
            float udc2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 37] << 8) + (int)this.fileRawData[index + 36]), GlobalData.ParamDefs[529]);
            float udc3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 39] << 8) + (int)this.fileRawData[index + 38]), GlobalData.ParamDefs[530]);
            float idc = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 41] << 8) + (int)this.fileRawData[index + 40]), GlobalData.ParamDefs[531]);
            float idc2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 43] << 8) + (int)this.fileRawData[index + 42]), GlobalData.ParamDefs[532]);
            float idc3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 45] << 8) + (int)this.fileRawData[index + 44]), GlobalData.ParamDefs[533]);
            byte fehlerWR = this.fileRawData[index + 46];
            byte fehlerWR2 = this.fileRawData[index + 47];
            byte fehlerWR3 = this.fileRawData[index + 48];
            byte statusWR = this.fileRawData[index + 49];
            byte statusWR2 = this.fileRawData[index + 50];
            byte statusWR3 = this.fileRawData[index + 51];
            int num = (int)this.fileRawData[index + 55] << 24;
            num += (int)this.fileRawData[index + 54] << 16;
            num += (int)this.fileRawData[index + 53] << 8;
            num += (int)this.fileRawData[index + 52];
            float ertragWR = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[492]);
            num = (int)this.fileRawData[index + 59] << 24;
            num += (int)this.fileRawData[index + 58] << 16;
            num += (int)this.fileRawData[index + 57] << 8;
            num += (int)this.fileRawData[index + 56];
            float ertragWR2 = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[493]);
            num = (int)this.fileRawData[index + 63] << 24;
            num += (int)this.fileRawData[index + 62] << 16;
            num += (int)this.fileRawData[index + 61] << 8;
            num += (int)this.fileRawData[index + 60];
            float ertragWR3 = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[494]);
            return new Constants.SP120Status(dcNennleistung, acLeistung, uac, uac2, uac3, uacPeak, uacPeak2, uacPeak3, pac, pac2, pac3, netzfrequenz, netzfrequenz2, netzfrequenz3, temperaturWR, temperaturWR2, temperaturWR3, udc, udc2, udc3, idc, idc2, idc3, fehlerWR, fehlerWR2, fehlerWR3, statusWR, statusWR2, statusWR3, ertragWR, ertragWR2, ertragWR3);
        }

        private Constants.SP300Status ReadSP300Block(int index)
        {
            ushort dcNennleistung = (ushort)(((int)this.fileRawData[index + 1] << 8) + (int)this.fileRawData[index]);
            ushort acLeistung = (ushort)(((int)this.fileRawData[index + 3] << 8) + (int)this.fileRawData[index + 2]);
            byte anzahlDCM = this.fileRawData[index + 4];
            float uac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 6] << 8) + (int)this.fileRawData[index + 5]), GlobalData.ParamDefs[355]);
            float uac2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 8] << 8) + (int)this.fileRawData[index + 7]), GlobalData.ParamDefs[356]);
            float uac3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 10] << 8) + (int)this.fileRawData[index + 9]), GlobalData.ParamDefs[357]);
            float uacPeak = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 12] << 8) + (int)this.fileRawData[index + 11]), GlobalData.ParamDefs[359]);
            float uacPeak2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 14] << 8) + (int)this.fileRawData[index + 13]), GlobalData.ParamDefs[360]);
            float uacPeak3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 16] << 8) + (int)this.fileRawData[index + 15]), GlobalData.ParamDefs[361]);
            float pac = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 18] << 8) + (int)this.fileRawData[index + 17]), GlobalData.ParamDefs[351]);
            float pac2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 20] << 8) + (int)this.fileRawData[index + 19]), GlobalData.ParamDefs[352]);
            float pac3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 22] << 8) + (int)this.fileRawData[index + 21]), GlobalData.ParamDefs[353]);
            float netzfrequenz = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 24] << 8) + (int)this.fileRawData[index + 23]), GlobalData.ParamDefs[376]);
            float netzfrequenz2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 26] << 8) + (int)this.fileRawData[index + 25]), GlobalData.ParamDefs[377]);
            float netzfrequenz3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 28] << 8) + (int)this.fileRawData[index + 27]), GlobalData.ParamDefs[378]);
            float temperaturWR = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 30] << 8) + (int)this.fileRawData[index + 29]), GlobalData.ParamDefs[371]);
            float temperaturWR2 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 32] << 8) + (int)this.fileRawData[index + 31]), GlobalData.ParamDefs[372]);
            float temperaturWR3 = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 34] << 8) + (int)this.fileRawData[index + 33]), GlobalData.ParamDefs[373]);
            float maxTemperaturDcm = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 36] << 8) + (int)this.fileRawData[index + 35]), GlobalData.ParamDefs[379]);
            float temperaturFP = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 38] << 8) + (int)this.fileRawData[index + 37]), GlobalData.ParamDefs[380]);
            float uzk = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 40] << 8) + (int)this.fileRawData[index + 39]), GlobalData.ParamDefs[374]);
            float uzkAbweichung = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 42] << 8) + (int)this.fileRawData[index + 41]), GlobalData.ParamDefs[375]);
            return new Constants.SP300Status(dcNennleistung, acLeistung, anzahlDCM, uac, uac2, uac3, uacPeak, uacPeak2, uacPeak3, pac, pac2, pac3, netzfrequenz, netzfrequenz2, netzfrequenz3, temperaturWR, temperaturWR2, temperaturWR3, maxTemperaturDcm, temperaturFP, uzk, uzkAbweichung);
        }

        private DcmStatus ReadDcmBlock(int index)
        {
            byte dcm = this.fileRawData[index];
            float upv = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 2] << 8) + (int)this.fileRawData[index + 1]), GlobalData.ParamDefs[403]);
            float ipv = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 4] << 8) + (int)this.fileRawData[index + 3]), GlobalData.ParamDefs[404]);
            byte status = this.fileRawData[index + 5];
            byte error = this.fileRawData[index + 6];
            float temp = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 8] << 8) + (int)this.fileRawData[index + 7]), GlobalData.ParamDefs[405]);
            int num = (int)this.fileRawData[index + 12] << 24;
            num += (int)this.fileRawData[index + 11] << 16;
            num += (int)this.fileRawData[index + 10] << 8;
            num += (int)this.fileRawData[index + 9];
            float ertrag = UtilityCore.Uint2Float((uint)num, GlobalData.ParamDefs[492]);
            float fw = UtilityCore.Uint2Float((uint)(((int)this.fileRawData[index + 14] << 8) + (int)this.fileRawData[index + 13]), GlobalData.ParamDefs[389]);
            return new DcmStatus(dcm, upv, ipv, status, error, temp, ertrag, fw);
        }

        private ConverterStatus? ExtractFile31()
        {
            if (this.fileRawData.Length < 15)
            {
                return null;
            }

            for (int i = 0; i < this.fileRawData.Length; i += 4)
            {
                Array.Reverse(this.fileRawData, i, 4);
            }
            if (this.fileRawData[13] != 31)
            {
                return null;
            }
            DateTime date = DateTime.MinValue;
            try
            {
                date = new DateTime(((int)this.fileRawData[3] << 8) + (int)this.fileRawData[2], (int)this.fileRawData[1], (int)this.fileRawData[0], (int)this.fileRawData[4], (int)this.fileRawData[5], (int)this.fileRawData[6]);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
            float fwVersion = (float)(((int)this.fileRawData[8] << 8) + (int)this.fileRawData[7]) / 100f;
            int num = ((int)this.fileRawData[10] << 8) + (int)this.fileRawData[9];
            ushort gKlasse = (ushort)(((int)this.fileRawData[12] << 8) + (int)this.fileRawData[11]);
            CommonStatus? commonStatus = null;
            ITypeStatus typeStatus = null;
            IList<DcmStatus> list = new List<DcmStatus>();
            int num2 = 15;
            while (this.fileRawData.Length > num2 + 3)
            {
                int num3 = (int)this.fileRawData[num2];
                int num4 = ((int)this.fileRawData[num2 + 2] << 8) + (int)this.fileRawData[num2 + 1];
                num2 += 3;
                if (this.fileRawData.Length >= num2 + num4)
                {
                    switch (num3)
                    {
                        case 1:
                            if (commonStatus.HasValue)
                            {
                                return null;
                            }
                            commonStatus = new CommonStatus?(this.ReadCommonBlock(num2));
                            break;

                        case 2:
                            if (typeStatus != null)
                            {
                                return null;
                            }
                            typeStatus = ReadSP50Block(num2);
                            break;

                        case 3:
                            if (typeStatus != null)
                            {
                                return null;
                            }
                            typeStatus = ReadSP300Block(num2);
                            break;

                        case 4:
                            {
                                if (num4 == 14)
                                {
                                    num4 = 15;
                                }
                                DcmStatus? dcmStatus = new DcmStatus?(this.ReadDcmBlock(num2));
                                if (dcmStatus.HasValue)
                                {
                                    list.Add(dcmStatus.Value);
                                }
                                break;
                            }
                        case 5:
                            if (typeStatus != null)
                            {
                                return null;
                            }
                            typeStatus = this.ReadSP120Block(num2);
                            break;

                        default:

                            break;
                    }
                }
                else
                {
                }
                num2 += num4;
            }
            if (!commonStatus.HasValue)
            {
                return null;
            }
            return new ConverterStatus?(new ConverterStatus(commonStatus.Value, date, fwVersion, num, gKlasse)
            {
                TypeStatus = typeStatus,
                DcmStatus = list
            });
        }

        private TransmissionState DoReadParameterIntern(int pkParameter, out uint value, int converterType)
        {
            byte[] array = new byte[13];
            value = 0u;

            int num = GlobalData.PKParameter2ConverterParameter(pkParameter, converterType);
            if (num == 0)
            {
                return TransmissionState.UnknownParameter;
            }

            byte[] array2;
            TransmissionState result;
            try
            {
                array2 = ProtocolFrame.CreateBuffer((ushort)_serialNumber, 0, (ushort)num, false, true, false, 0u);
                this.WriteData(array2, 0, array2.Length);
            }
            catch (TimeoutException)
            {
                result = TransmissionState.WriteTimeout;
                return result;
            }
            catch (Exception ex)
            {
                result = TransmissionState.Error;
                return result;
            }
            try
            {
                TransmissionState transmissionState = this.ReadFrame(13, array2, array);
                if (transmissionState == TransmissionState.Ok)
                {
                    value = (uint)(((int)array[7] << 24) + ((int)array[8] << 16) + ((int)array[9] << 8) + (int)array[10]);
                    result = TransmissionState.Ok;
                }
                else
                {
                    result = transmissionState;
                }
            }
            catch (TimeoutException)
            {
                result = TransmissionState.ReadTimeout;
            }
            catch (Exception ex2)
            {
                result = TransmissionState.Error;
            }

            return result;
        }

        private TransmissionState ReadFrame(int blockLength, byte[] outBuffer, byte[] inBuffer)
        {
            return this.ReadFrame(blockLength, outBuffer, inBuffer, 0);
        }

        private TransmissionState ReadFrame(int blockLength, byte[] outBuffer, byte[] inBuffer, int timeout)
        {
            TcpClient tcpClient = null;
            NetworkStream networkStream = null;
            Queue<byte> queue = new Queue<byte>(blockLength);
            DateTime now = DateTime.Now;
            byte[] array;
            DateTime t;

            tcpClient = _client;
            networkStream = _clientStream;
            array = new byte[tcpClient.ReceiveBufferSize];
            t = now.AddMilliseconds((double)Math.Max(Settings.Default.ReadFrameTcpTimeout, timeout));

            while (DateTime.Now < t)
            {
                int num = 0;
                while (queue.Count + num < blockLength && DateTime.Now < t)
                {
                    if (tcpClient != null)
                    {
                        num = tcpClient.Available;
                    }

                    if (num == 0)
                    {
                        Thread.Sleep(50);
                    }
                }

                if (tcpClient != null)
                {
                    num = tcpClient.Available;
                }

                if (queue.Count + num >= blockLength)
                {
                    int num3 = blockLength - queue.Count;
                    if (tcpClient != null && networkStream != null)
                    {
                        networkStream.Read(array, 0, num3);
                    }

                    if (this.DataReceived != null)
                    {
                        byte[] array2 = new byte[num3];
                        Array.Copy(array, array2, num3);
                        try
                        {
                            this.DataReceived(array2);
                        }
                        catch
                        {
                        }
                    }
                    for (int i = 0; i < num3; i++)
                    {
                        queue.Enqueue(array[i]);
                    }
                    while (queue.Count > 0 && queue.Peek() != 2)
                    {
                        queue.Dequeue();
                        if (tcpClient != null)
                        {
                            if (tcpClient.Available > 0 && networkStream != null)
                            {
                                array[0] = (byte)networkStream.ReadByte();
                                queue.Enqueue(array[0]);
                            }
                        }
                    }
                    if (queue.Count >= blockLength)
                    {
                        queue.CopyTo(inBuffer, 0);
                        if (inBuffer[0] != 2 || inBuffer[blockLength - 1] != 3)
                        {
                            return TransmissionState.FrameError;
                        }
                        if (inBuffer[blockLength - 2] != GlobalData.CalcChecksum(inBuffer, 1, blockLength - 3))
                        {
                            return TransmissionState.ChecksumError;
                        }
                        if (inBuffer[1] != outBuffer[3] || inBuffer[2] != outBuffer[4])
                        {
                            return TransmissionState.AddressError;
                        }
                        if (inBuffer[3] != outBuffer[1] || inBuffer[4] != outBuffer[2])
                        {
                            return TransmissionState.AddressError;
                        }
                        if ((inBuffer[5] & 3) != (outBuffer[5] & 3) || inBuffer[6] != outBuffer[6])
                        {
                            return TransmissionState.WrongAnswer;
                        }
                        if ((inBuffer[5] & 16) == 0)
                        {
                            return TransmissionState.Ok;
                        }
                        switch (inBuffer[10])
                        {
                            case 1:
                                return TransmissionState.UnknownParameter;

                            case 2:
                                return TransmissionState.PasswordProtected;

                            case 3:
                                return TransmissionState.MaxValueError;

                            case 4:
                                return TransmissionState.MinValueError;

                            case 5:
                                return TransmissionState.ParameterIsReadOnly;

                            case 6:
                                return TransmissionState.AccessCode1;

                            case 7:
                                return TransmissionState.AccessCode2;

                            case 8:
                                return TransmissionState.NoCommand;

                            case 9:
                                return TransmissionState.WrongValue;

                            case 10:
                                return TransmissionState.NoAnswer;

                            case 11:
                                return TransmissionState.NoFWFile;

                            default:
                                return TransmissionState.Error;
                        }
                    }
                }
                Thread.Sleep(0);
            }
            return TransmissionState.ReadTimeout;
        }

        private TransmissionState DoWriteParameterIntern(int pkParameter, int converterType, ref uint value)
        {
            byte[] array = new byte[13];
            int num = GlobalData.PKParameter2ConverterParameter(pkParameter, converterType);

            byte[] array2;
            TransmissionState result;
            try
            {
                array2 = ProtocolFrame.CreateBuffer((ushort)_serialNumber, 0, (ushort)num, true, false, false, value);
                this.WriteData(array2, 0, array2.Length);
            }
            catch (TimeoutException)
            {
                result = TransmissionState.WriteTimeout;
                return result;
            }
            catch (Exception ex)
            {
                result = TransmissionState.Error;
                return result;
            }
            try
            {
                TransmissionState transmissionState = this.ReadFrame(13, array2, array);
                if (transmissionState == TransmissionState.Ok)
                {
                    uint num2 = (uint)(((int)array[7] << 24) + ((int)array[8] << 16) + ((int)array[9] << 8) + (int)array[10]);
                    if (value == num2)
                    {
                        result = TransmissionState.Ok;
                    }
                    else
                    {
                        value = num2;
                        result = TransmissionState.Error;
                    }
                }
                else
                {
                    result = transmissionState;
                }
            }
            catch (TimeoutException)
            {
                result = TransmissionState.ReadTimeout;
            }
            catch (Exception ex2)
            {
                result = TransmissionState.Error;
            }
            return result;
        }

        private void FileData(byte[] values, uint index, uint length, uint fileNumber)
        {
            byte[] destinationArray;

            int num = (int)fileNumber;
            if (num <= 18)
            {
                if (num == 12)
                {
                    destinationArray = this.file12Data;
                    goto IL_68;
                }
                if (num == 18)
                {
                    destinationArray = this.file18Data;
                    goto IL_68;
                }
            }
            else
            {
                switch (num)
                {
                    case 23:
                    case 24:
                        break;

                    default:
                        if (num == 28)
                        {
                            destinationArray = this.file28Data;
                            goto IL_68;
                        }
                        if (num != 31)
                        {
                        }
                        break;
                }
            }
            destinationArray = this.fileRawData;

            IL_68:
            Array.Copy(values, 0L, destinationArray, (long)((ulong)index), (long)((ulong)length));
        }

        private TransmissionState DoReadFile(uint fileNumber, int converterType)
        {
            uint num = 0u;
            uint num2 = 0u;
            byte[] values = new byte[128];
            int num3 = Settings.Default.ReadFileLengthRepeat;

            int num4 = (int)fileNumber;
            if (num4 <= 12)
            {
                if (num4 != 2)
                {
                    if (num4 == 12)
                    {
                        this.file12Data = new byte[32768];
                    }
                }
            }
            else
            {
                if (num4 != 18)
                {
                    if (num4 == 28)
                    {
                        this.file28Data = new byte[32768];
                    }
                }
                else
                {
                    this.file18Data = new byte[1480];
                }
            }
            uint v = 0;
            TransmissionState transmissionState = DoWriteParameterIntern(260, converterType, ref v);
            if (transmissionState == TransmissionState.Ok)
            {
                transmissionState = this.DoWriteParameterIntern(260, converterType, ref fileNumber);
            }
            DateTime t = DateTime.Now.AddMilliseconds((double)Settings.Default.ReadFileLengthTimeout);
            while (transmissionState == TransmissionState.Ok && num == 0u && num3 > 0 && t > DateTime.Now)
            {
                transmissionState = this.DoReadParameter(261, out num, converterType);
                System.Threading.Thread.Sleep(50);
                num3--;
            }

            num *= 4u;
            uint num5 = num;
            if (fileNumber == 23 || fileNumber == 24 || fileNumber == 31)
            {
                this.fileRawData = new byte[num];
            }
            if (transmissionState == TransmissionState.Ok && num == 0u)
            {
                transmissionState = TransmissionState.ReadTimeout;
            }
            if (transmissionState == TransmissionState.Ok)
            {
                while (transmissionState == TransmissionState.Ok)
                {
                    if (num5 <= 0u)
                    {
                        break;
                    }
                    Thread.Sleep(50);

                    transmissionState = this.ReadFileBlock(num2, values, converterType);
                    if (num5 >= 128u)
                    {
                        FileData(values, num2, 128u, fileNumber);
                        num5 -= 128u;
                    }
                    else
                    {
                        FileData(values, num2, num5, fileNumber);
                        num5 = 0u;
                    }
                    num2 += 128u;
                }
            }

            return transmissionState;
        }

        private TransmissionState ReadFileBlock(uint uiFileIndex, byte[] values, int converterType)
        {
            int num = Settings.Default.ReadFileBlockRepeat;
            TransmissionState transmissionState = this.ReadFileBlockIntern(values, converterType);
            while (transmissionState != TransmissionState.Ok && --num >= 0)
            {
                uint v = uiFileIndex / 4u;
                transmissionState = this.DoWriteParameterIntern(264, 0, ref v);
                if (transmissionState == TransmissionState.Ok)
                {
                    transmissionState = this.ReadFileBlockIntern(values, converterType);
                }
            }
            return transmissionState;
        }

        private TransmissionState ReadFileBlockIntern(byte[] values, int converterType)
        {
            byte[] array = new byte[137];
            int num = GlobalData.PKParameter2ConverterParameter(270, converterType);

            byte[] array2;
            TransmissionState result;
            try
            {
                array2 = ProtocolFrame.CreateBuffer((ushort)_serialNumber, 0, (ushort)num, false, true, false, 0u);
                this.WriteData(array2);
            }
            catch (TimeoutException)
            {
                result = TransmissionState.WriteTimeout;
                return result;
            }
            catch (Exception ex)
            {
                result = TransmissionState.Error;
                return result;
            }
            try
            {
                TransmissionState transmissionState = this.ReadFrame(137, array2, array);
                if (transmissionState == TransmissionState.Ok)
                {
                    Array.Copy(array, 7, values, 0, 128);
                    result = TransmissionState.Ok;
                }
                else
                {
                    result = transmissionState;
                }
            }
            catch (TimeoutException)
            {
                result = TransmissionState.ReadTimeout;
            }
            catch (Exception ex2)
            {
                result = TransmissionState.Error;
            }
            return result;
        }

        public ConverterStatus? ReadData(uint readTipology)
        {
            if (DoReadFile(readTipology, 60) != TransmissionState.Ok)
            {
                return null;
            }
            return ExtractFile31();
        }

        public TransmissionState Connect()
        {
            return ConnectTcpIndividual();
        }

        public void Init(Hashtable ps)
        {
            _serialNumber = Convert.ToInt32(ps[InverterCommonProperties.SERIAL_NUMBER]);
            _endPoint = new IPEndPoint(IPAddress.Parse(Convert.ToString(ps[InverterCommonProperties.NET_IP_ADDRESS])), Convert.ToInt32(ps[InverterCommonProperties.NET_IP_PORT]));

            _client = new TcpClient()
            {
                SendBufferSize = 13,
                ReceiveBufferSize = 137,
                NoDelay = true,
                ReceiveTimeout = 10000,
                SendTimeout = 10000
            };
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_clientStream != null)
            {
                _clientStream.Close();
            }
            if (_client != null)
            {
                _client.Close();
            }
        }

        #endregion IDisposable Members

        #region IInverter Members

        public string SerialeNumber
        {
            get { return _serialNumber.ToString(); }
        }

        #endregion IInverter Members
    }
}