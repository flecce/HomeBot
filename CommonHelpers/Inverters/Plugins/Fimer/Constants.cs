using CommonHelpers.Inverters.Interfaces;
using System;

namespace CommonHelpers.Inverters.Plugins.Fimer
{
    public sealed class Constants
    {
        public class SP50Status : ITypeStatus
        {
            public ushort PotenzaProdottaGiornaliera { get; set; } // Prodotta giornata
            public ushort PotenzaCorrente { get; set; } // Potenza corrente
            public readonly float Netzfrequenz;
            public readonly float UDC;
            public readonly float UAC;
            public readonly float IDC;
            public readonly float IAC;
            public readonly float Geraetetemperatur; // Temperatura
            public readonly float Netzimpedand;
            public readonly float Fehlstrom;
            public readonly float Einstrahlung;
            public readonly float Modultemperatur;
            public readonly float Sensorertrag;

            public SP50Status(ushort dcNennleistung, ushort acLeistung, float netzfrequenz, float udc, float uac, float idc, float iac, float geraetetemperatur, float netzimpedand, float fehlstrom, float einstrahlung, float modultemperatur, float sensorertrag)
            {
                PotenzaProdottaGiornaliera = dcNennleistung;
                PotenzaCorrente = acLeistung;
                this.Netzfrequenz = netzfrequenz;
                this.UDC = udc;
                this.UAC = uac;
                this.IDC = idc;
                this.IAC = iac;
                this.Geraetetemperatur = geraetetemperatur;
                this.Netzimpedand = netzimpedand;
                this.Fehlstrom = fehlstrom;
                this.Einstrahlung = einstrahlung;
                this.Modultemperatur = modultemperatur;
                this.Sensorertrag = sensorertrag;
            }

            public object GetProperty(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                        return Geraetetemperatur;

                    case Enums.CommonPropertyType.Temperatura2:
                        return Modultemperatur;

                    case Enums.CommonPropertyType.CorrenteDiGuasto:
                        return Fehlstrom;

                    case Enums.CommonPropertyType.IAC:
                        return IAC;

                    case Enums.CommonPropertyType.IDC:
                        return IDC;

                    case Enums.CommonPropertyType.ProduzioneCorrente:
                        return PotenzaCorrente;

                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                        return PotenzaProdottaGiornaliera;

                    case Enums.CommonPropertyType.UAC:
                        return UAC;

                    case Enums.CommonPropertyType.UDC:
                        return UDC;
                }
                throw new NotSupportedException("Property not supported:" + t.ToString());
            }

            public bool PropertyIsSupported(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                    case Enums.CommonPropertyType.Temperatura2:
                    case Enums.CommonPropertyType.CorrenteDiGuasto:
                    case Enums.CommonPropertyType.IAC:
                    case Enums.CommonPropertyType.IDC:
                    case Enums.CommonPropertyType.ProduzioneCorrente:
                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                    case Enums.CommonPropertyType.UAC:
                    case Enums.CommonPropertyType.UDC:
                        return true;
                }
                return false;
            }
        }

        public class SP300Status : ITypeStatus
        {
            public ushort DCNennleistung { get; set; } // Prodotta giornata
            public ushort ACLeistung { get; set; } // Potenza corrente
            public readonly byte AnzahlDCM;
            public readonly float UAC1;
            public readonly float UAC2;
            public readonly float UAC3;
            public readonly float UACpeak1;
            public readonly float UACpeak2;
            public readonly float UACpeak3;
            public readonly float PAC1;
            public readonly float PAC2;
            public readonly float PAC3;
            public readonly float Netzfrequenz1;
            public readonly float Netzfrequenz2;
            public readonly float Netzfrequenz3;
            public readonly float TemperaturWR1;
            public readonly float TemperaturWR2;
            public readonly float TemperaturWR3;
            public readonly float MaxTemperaturDcm;
            public readonly float TemperaturFP;
            public readonly float UZK;
            public readonly float UZKabweichung;

            public SP300Status(ushort dcNennleistung, ushort acLeistung, byte anzahlDCM, float uac1, float uac2, float uac3, float uacPeak1, float uacPeak2, float uacPeak3, float pac1, float pac2, float pac3, float netzfrequenz1, float netzfrequenz2, float netzfrequenz3, float temperaturWR1, float temperaturWR2, float temperaturWR3, float maxTemperaturDcm, float temperaturFP, float uzk, float uzkAbweichung)
            {
                DCNennleistung = dcNennleistung;
                ACLeistung = acLeistung;
                this.AnzahlDCM = anzahlDCM;
                this.UAC1 = uac1;
                this.UAC2 = uac2;
                this.UAC3 = uac3;
                this.UACpeak1 = uacPeak1;
                this.UACpeak2 = uacPeak2;
                this.UACpeak3 = uacPeak3;
                this.PAC1 = pac1;
                this.PAC2 = pac2;
                this.PAC3 = pac3;
                this.Netzfrequenz1 = netzfrequenz1;
                this.Netzfrequenz2 = netzfrequenz2;
                this.Netzfrequenz3 = netzfrequenz3;
                this.TemperaturWR1 = temperaturWR1;
                this.TemperaturWR2 = temperaturWR2;
                this.TemperaturWR3 = temperaturWR3;
                this.MaxTemperaturDcm = maxTemperaturDcm;
                this.TemperaturFP = temperaturFP;
                this.UZK = uzk;
                this.UZKabweichung = uzkAbweichung;
            }

            public object GetProperty(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                        return TemperaturWR1;

                    case Enums.CommonPropertyType.Temperatura2:
                        return TemperaturWR2;

                    case Enums.CommonPropertyType.ProduzioneCorrente:
                        return ACLeistung;

                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                        return DCNennleistung;

                    case Enums.CommonPropertyType.UAC:
                        return UAC1;
                }
                throw new NotSupportedException("Property not supported:" + t.ToString());
            }

            public bool PropertyIsSupported(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                    case Enums.CommonPropertyType.Temperatura2:
                    case Enums.CommonPropertyType.ProduzioneCorrente:
                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                    case Enums.CommonPropertyType.UAC:
                        return true;
                }
                return false;
            }
        }

        public class SP120Status : ITypeStatus
        {
            public ushort DCNennleistung { get; set; } // Prodotta giornata
            public ushort ACLeistung { get; set; } // Potenza corrente
            public readonly float UAC1;
            public readonly float UAC2;
            public readonly float UAC3;
            public readonly float UACpeak1;
            public readonly float UACpeak2;
            public readonly float UACpeak3;
            public readonly float PAC1;
            public readonly float PAC2;
            public readonly float PAC3;
            public readonly float Netzfrequenz1;
            public readonly float Netzfrequenz2;
            public readonly float Netzfrequenz3;
            public readonly float TemperaturWR1;
            public readonly float TemperaturWR2;
            public readonly float TemperaturWR3;
            public readonly float UDC1;
            public readonly float UDC2;
            public readonly float UDC3;
            public readonly float IDC1;
            public readonly float IDC2;
            public readonly float IDC3;
            public readonly byte FehlerWR1;
            public readonly byte FehlerWR2;
            public readonly byte FehlerWR3;
            public readonly byte StatusWR1;
            public readonly byte StatusWR2;
            public readonly byte StatusWR3;
            public readonly float ErtragWR1;
            public readonly float ErtragWR2;
            public readonly float ErtragWR3;

            public SP120Status(ushort dcNennleistung, ushort acLeistung, float uac1, float uac2, float uac3, float uacPeak1, float uacPeak2, float uacPeak3, float pac1, float pac2, float pac3, float netzfrequenz1, float netzfrequenz2, float netzfrequenz3, float temperaturWR1, float temperaturWR2, float temperaturWR3, float udc1, float udc2, float udc3, float idc1, float idc2, float idc3, byte fehlerWR1, byte fehlerWR2, byte fehlerWR3, byte statusWR1, byte statusWR2, byte statusWR3, float ertragWR1, float ertragWR2, float ertragWR3)
            {
                DCNennleistung = dcNennleistung;
                ACLeistung = acLeistung;
                this.UAC1 = uac1;
                this.UAC2 = uac2;
                this.UAC3 = uac3;
                this.UACpeak1 = uacPeak1;
                this.UACpeak2 = uacPeak2;
                this.UACpeak3 = uacPeak3;
                this.PAC1 = pac1;
                this.PAC2 = pac2;
                this.PAC3 = pac3;
                this.Netzfrequenz1 = netzfrequenz1;
                this.Netzfrequenz2 = netzfrequenz2;
                this.Netzfrequenz3 = netzfrequenz3;
                this.TemperaturWR1 = temperaturWR1;
                this.TemperaturWR2 = temperaturWR2;
                this.TemperaturWR3 = temperaturWR3;
                this.UDC1 = udc1;
                this.UDC2 = udc2;
                this.UDC3 = udc3;
                this.IDC1 = idc1;
                this.IDC2 = idc2;
                this.IDC3 = idc3;
                this.FehlerWR1 = fehlerWR1;
                this.FehlerWR2 = fehlerWR2;
                this.FehlerWR3 = fehlerWR3;
                this.StatusWR1 = statusWR1;
                this.StatusWR2 = statusWR2;
                this.StatusWR3 = statusWR3;
                this.ErtragWR1 = ertragWR1;
                this.ErtragWR2 = ertragWR2;
                this.ErtragWR3 = ertragWR3;
            }

            public object GetProperty(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                        return TemperaturWR1;

                    case Enums.CommonPropertyType.Temperatura2:
                        return TemperaturWR2;

                    case Enums.CommonPropertyType.ProduzioneCorrente:
                        return ACLeistung;

                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                        return DCNennleistung;

                    case Enums.CommonPropertyType.UDC:
                        return UDC1;
                }
                throw new NotSupportedException("Property not supported:" + t.ToString());
            }

            public bool PropertyIsSupported(Enums.CommonPropertyType t)
            {
                switch (t)
                {
                    case Enums.CommonPropertyType.Temperatura1:
                    case Enums.CommonPropertyType.Temperatura2:
                    case Enums.CommonPropertyType.ProduzioneCorrente:
                    case Enums.CommonPropertyType.ProduzioneGiornaliera:
                    case Enums.CommonPropertyType.UDC:
                        return true;
                }
                return false;
            }
        }
    }
}