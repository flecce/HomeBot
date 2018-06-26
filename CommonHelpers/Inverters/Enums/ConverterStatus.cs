using CommonHelpers.Inverters.Interfaces;
using System;
using System.Collections.Generic;

namespace CommonHelpers.Inverters.Enums
{
    public struct ConverterStatus
    {
        public readonly DateTime Date;
        public readonly float FWVersion;
        public readonly int Seriennr;
        public readonly ushort Geraeteklasse;
        public readonly CommonStatus CommonStatus;
        private ITypeStatus typeStatus;
        private List<DcmStatus> dcmStatus;

        public ITypeStatus TypeStatus
        {
            get
            {
                return this.typeStatus;
            }
            set
            {
                if (this.typeStatus != null)
                {
                    throw new ArgumentException("is readonly", "TypeStatus");
                }
                this.typeStatus = value;
            }
        }

        public IList<DcmStatus> DcmStatus
        {
            get
            {
                if (this.dcmStatus != null)
                {
                    return this.dcmStatus.AsReadOnly();
                }
                return new List<DcmStatus>(0);
            }
            set
            {
                if (this.dcmStatus != null)
                {
                    throw new ArgumentException("is readonly", "DcmStatus");
                }
                this.dcmStatus = new List<DcmStatus>(value);
            }
        }

        public ConverterStatus(CommonStatus commonStatus, DateTime date, float fwVersion, int seriennr, ushort gKlasse)
        {
            this.CommonStatus = commonStatus;
            this.Date = date;
            this.FWVersion = fwVersion;
            this.Seriennr = seriennr;
            this.Geraeteklasse = gKlasse;
            this.dcmStatus = null;
            this.typeStatus = null;
        }
    }
}