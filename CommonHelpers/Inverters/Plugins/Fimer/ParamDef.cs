using System.Threading;

namespace CommonHelpers.Inverters.Plugins.Fimer
{
    public class ParamDef
    {
        private int paramNr;
        private string strShort;
        private int unitID;
        private int postCommaDigits;
        private int pwLevel;
        private bool isSigned;
        private int type;
        private int loggingLevel;
        private int framAddr;
        private bool is32Bit;
        private int submenuID;
        private int paramNrSP50;
        private int paramNrSP120;
        private int paramNrSP300;
        private string strGerman;
        private string strEnglish;
        private string strSpanish;
        private string strTurkish;
        private string strCzech;
        private string strFrench;
        private string strItalian;

        public int ParamNr
        {
            get
            {
                return this.paramNr;
            }
            set
            {
                this.paramNr = value;
            }
        }

        public string Short
        {
            get
            {
                return this.strShort;
            }
        }

        public string Name
        {
            get
            {
                string name = Thread.CurrentThread.CurrentUICulture.Name;
                if (name.StartsWith("de"))
                {
                    return this.NameGerman;
                }
                if (name.StartsWith("es"))
                {
                    return this.NameSpanish;
                }
                if (name.StartsWith("tr"))
                {
                    return this.NameTurkish;
                }
                if (name.StartsWith("cz"))
                {
                    return this.NameCzech;
                }
                if (name.StartsWith("fr"))
                {
                    return this.NameFrench;
                }
                if (name.StartsWith("it"))
                {
                    return this.NameItalian;
                }
                return this.NameEnglish;
            }
        }

        public string NameGerman
        {
            get
            {
                return this.strGerman;
            }
        }

        public string NameEnglish
        {
            get
            {
                return this.strEnglish;
            }
        }

        public string NameSpanish
        {
            get
            {
                return this.strSpanish;
            }
        }

        public string NameTurkish
        {
            get
            {
                return this.strTurkish;
            }
        }

        public string NameCzech
        {
            get
            {
                return this.strCzech;
            }
        }

        public string NameFrench
        {
            get
            {
                return this.strFrench;
            }
        }

        public string NameItalian
        {
            get
            {
                return this.strItalian;
            }
        }

        public int UnitID
        {
            get
            {
                return this.unitID;
            }
        }

        public int PostCommaDigits
        {
            get
            {
                return this.postCommaDigits;
            }
        }

        public int PWLevel
        {
            get
            {
                return this.pwLevel;
            }
        }

        public bool IsSigned
        {
            get
            {
                return this.isSigned;
            }
        }

        public int Type
        {
            get
            {
                return this.type;
            }
        }

        public int LoggingLevel
        {
            get
            {
                return this.loggingLevel;
            }
        }

        public int FRAMAddr
        {
            get
            {
                return this.framAddr;
            }
        }

        public bool Is32Bit
        {
            get
            {
                return this.is32Bit;
            }
        }

        public int SubMenuID
        {
            get
            {
                return this.submenuID;
            }
        }

        public int ParamNrSP50
        {
            get
            {
                return this.paramNrSP50;
            }
        }

        public int ParamNrSP120
        {
            get
            {
                return this.paramNrSP120;
            }
        }

        public int ParamNrSP300
        {
            get
            {
                return this.paramNrSP300;
            }
        }

        public ParamDef(int paramNr, string strShort, int unitID, int postCommaDigits, int pwLevel, bool isSigned, int type, int loggingLevel, int framAddr, bool is32Bit, int submenuID, int paramNrSP50, int paramNrSP120, int paramNrSP300, string strGerman, string strEnglish, string strSpanish, string strTurkish, string strCzech, string strFrench, string strItalian)
        {
            this.paramNr = paramNr;
            this.strShort = strShort;
            this.unitID = unitID;
            this.postCommaDigits = postCommaDigits;
            this.pwLevel = pwLevel;
            this.isSigned = isSigned;
            this.type = type;
            this.loggingLevel = loggingLevel;
            this.framAddr = framAddr;
            this.is32Bit = is32Bit;
            this.submenuID = submenuID;
            this.paramNrSP50 = paramNrSP50;
            this.paramNrSP120 = paramNrSP120;
            this.paramNrSP300 = paramNrSP300;
            this.strGerman = strGerman;
            this.strEnglish = strEnglish;
            this.strSpanish = strSpanish;
            this.strTurkish = strTurkish;
            this.strCzech = strCzech;
            this.strFrench = strFrench;
            this.strItalian = strItalian;
        }

        public int ParameterNr2ConverterParameter(int converterType)
        {
            if (converterType > 36)
            {
                if (converterType <= 100)
                {
                    switch (converterType)
                    {
                        case 50:
                        case 51:
                        case 55:
                        case 56:
                            goto IL_7A;
                        case 52:
                        case 53:
                        case 54:
                            goto IL_96;
                        default:
                            if (converterType != 100)
                            {
                                goto IL_96;
                            }
                            break;
                    }
                }
                else
                {
                    if (converterType != 120)
                    {
                        if (converterType != 300)
                        {
                            goto IL_96;
                        }
                        return this.ParamNrSP300;
                    }
                }
                return this.ParamNrSP120;
            }
            if (converterType == 1)
            {
                return this.ParamNrSP50;
            }
            switch (converterType)
            {
                case 25:
                case 26:
                    break;

                default:
                    switch (converterType)
                    {
                        case 35:
                        case 36:
                            break;

                        default:
                            goto IL_96;
                    }
                    break;
            }
            IL_7A:
            return this.ParamNrSP50;
            IL_96:
            return this.ParamNr;
        }
    }
}