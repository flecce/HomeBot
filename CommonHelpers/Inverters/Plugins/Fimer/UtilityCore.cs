using System;

namespace CommonHelpers.Inverters.Plugins.Fimer
{
    public sealed class UtilityCore
    {
        public static float Uint2Float(uint uiValue, ParamDef rowDef)
        {
            int num;
            switch (rowDef.PostCommaDigits)
            {
                case 0:
                    num = 1;
                    break;

                case 1:
                    num = 10;
                    break;

                case 2:
                    num = 100;
                    break;

                case 3:
                    num = 1000;
                    break;

                case 4:
                    num = 10000;
                    break;

                default:
                    num = (int)Math.Pow(10.0, (double)rowDef.PostCommaDigits);
                    break;
            }
            float result;
            if (rowDef.Is32Bit)
            {
                if (rowDef.IsSigned)
                {
                    result = (float)uiValue / (float)num;
                }
                else
                {
                    result = uiValue / (float)num;
                }
            }
            else
            {
                if (rowDef.IsSigned)
                {
                    result = (float)((short)uiValue) / (float)num;
                }
                else
                {
                    result = uiValue / (float)num;
                }
            }
            return result;
        }

        public static string Uint2String(uint uiValue, ParamDef rowDef)
        {
            return string.Format("{0:F" + rowDef.PostCommaDigits + "}", Uint2Float(uiValue, rowDef));
        }

        public static uint String2Uint(string strValue, ParamDef rowDef)
        {
            uint result = 0u;
            try
            {
                int num;
                switch (rowDef.PostCommaDigits)
                {
                    case 0:
                        num = 1;
                        break;

                    case 1:
                        num = 10;
                        break;

                    case 2:
                        num = 100;
                        break;

                    case 3:
                        num = 1000;
                        break;

                    case 4:
                        num = 10000;
                        break;

                    default:
                        num = (int)Math.Pow(10.0, (double)rowDef.PostCommaDigits);
                        break;
                }
                float num2 = float.Parse(strValue) * (float)num + 0.5f;
                if (rowDef.Is32Bit)
                {
                    if (rowDef.IsSigned)
                    {
                        result = (uint)((int)num2);
                    }
                    else
                    {
                        result = (uint)num2;
                    }
                }
                else
                {
                    if (rowDef.IsSigned)
                    {
                        result = (uint)((short)num2);
                    }
                    else
                    {
                        result = (uint)((ushort)num2);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
    }
}