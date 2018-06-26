namespace CommonHelpers.Inverters.Enums
{
    public struct DcmStatus
    {
        public readonly byte Dcm;
        public readonly float UPV;
        public readonly float IPV;
        public readonly byte Status;
        public readonly byte Error;
        public readonly float Temperatur;
        public readonly float Ertrag;
        public readonly float FWVersion;

        public DcmStatus(byte dcm, float upv, float ipv, byte status, byte error, float temp, float ertrag, float fw)
        {
            this.Dcm = dcm;
            this.UPV = upv;
            this.IPV = ipv;
            this.Status = status;
            this.Error = error;
            this.Temperatur = temp;
            this.Ertrag = ertrag;
            this.FWVersion = fw;
        }
    }
}