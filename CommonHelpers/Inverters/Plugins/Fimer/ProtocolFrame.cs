using CommonHelpers.Inverters.Enums;
using System;

using System.Text;

namespace CommonHelpers.Inverters.Plugins.Fimer
{
    public struct ProtocolFrame
    {
        public readonly TransmissionState State;
        public readonly ushort Receiver;
        public readonly ushort Sender;
        public readonly ushort Parameter;
        public readonly bool Write;
        public readonly bool Read;
        public readonly bool Master;
        public readonly bool Error;
        public readonly bool File;
        public readonly bool Vdew;
        public readonly byte Checksum;
        public readonly byte RatedChecksum;
        public readonly int DataLength;
        public readonly DateTime Time;
        private readonly byte[] buffer;

        public byte[] Buffer
        {
            get
            {
                byte[] array = new byte[this.buffer.Length];
                Array.Copy(this.buffer, array, array.Length);
                return array;
            }
        }

        public byte[] Value
        {
            get
            {
                byte[] array = new byte[this.DataLength];
                Array.Copy(this.buffer, 7, array, 0, this.DataLength);
                return array;
            }
        }

        public uint ParameterValue
        {
            get
            {
                if (this.DataLength != 4)
                {
                    throw new NotSupportedException("Protocol Frame ist Not a 4-Byte Protocol!");
                }
                return (uint)(((int)this.buffer[7] << 24) + ((int)this.buffer[8] << 16) + ((int)this.buffer[9] << 8) + (int)this.buffer[10]);
            }
        }

        public int Length
        {
            get
            {
                return this.buffer.Length;
            }
        }

        public ProtocolFrame(ushort receiver, ushort sender, ushort parameter, bool write, bool read, bool master, uint value)
        {
            this = new ProtocolFrame(receiver, sender, parameter, write, read, master, false, false, false, ProtocolFrame.ToByte(value));
        }

        public ProtocolFrame(ushort receiver, ushort sender, ushort parameter, bool write, bool read, bool master, bool error, bool file, bool vdew, byte[] value)
        {
            this.Time = DateTime.Now;
            this.buffer = ProtocolFrame.CreateBuffer(receiver, sender, parameter, write, read, master, error, file, vdew, value);
            this.DataLength = value.Length;
            this.Checksum = this.buffer[7 + this.DataLength];
            this.RatedChecksum = this.Checksum;
            this.Receiver = receiver;
            this.Sender = sender;
            this.Parameter = parameter;
            this.Write = write;
            this.Read = read;
            this.Master = master;
            this.Error = error;
            this.File = file;
            this.Vdew = vdew;
            this.State = TransmissionState.Ok;
        }

        public ProtocolFrame(byte[] data, int startIndex, int length)
        {
            this.Time = DateTime.Now;
            if (length < 13)
            {
                throw new ArgumentOutOfRangeException("length", length, "must not be less then " + 13);
            }
            if (data[startIndex] != 2)
            {
                throw new ApplicationException("FrameErrorException");
            }
            this.State = TransmissionState.Ok;
            this.Receiver = (ushort)(((int)data[startIndex + 1] << 8) + (int)data[startIndex + 2]);
            this.Sender = (ushort)(((int)data[startIndex + 3] << 8) + (int)data[startIndex + 4]);
            ushort num = (ushort)(((int)data[startIndex + 5] << 8) + (int)data[startIndex + 6]);
            this.Parameter = (ushort)(num & 1023);
            this.Write = ((num & 32768) == 32768);
            this.Read = ((num & 16384) == 16384);
            this.Master = ((num & 8192) == 8192);
            this.Error = ((num & 4096) == 4096);
            this.File = ((num & 2048) == 2048);
            this.Vdew = ((num & 1024) == 1024);
            int num2;
            if (this.File)
            {
                if (length < 137)
                {
                    throw new ApplicationException("FrameErrorException");
                }
                num2 = 137;
            }
            else
            {
                num2 = 13;
            }
            if (data[startIndex + num2 - 1] != 3)
            {
                throw new ApplicationException("FrameErrorException");
            }
            this.Checksum = data[startIndex + num2 - 2];
            this.RatedChecksum = GlobalData.CalcChecksum(data, startIndex + 1, startIndex + num2 - 3);
            if (this.RatedChecksum != this.Checksum)
            {
                this.State = TransmissionState.ChecksumError;
            }
            if (this.State == TransmissionState.Ok && this.Error)
            {
                switch (data[startIndex + 10])
                {
                    case 1:
                        this.State = TransmissionState.UnknownParameter;
                        break;

                    case 2:
                        this.State = TransmissionState.PasswordProtected;
                        break;

                    case 3:
                        this.State = TransmissionState.MaxValueError;
                        break;

                    case 4:
                        this.State = TransmissionState.MinValueError;
                        break;

                    case 5:
                        this.State = TransmissionState.ParameterIsReadOnly;
                        break;

                    case 6:
                        this.State = TransmissionState.AccessCode1;
                        break;

                    case 7:
                        this.State = TransmissionState.AccessCode2;
                        break;

                    case 8:
                        this.State = TransmissionState.NoCommand;
                        break;

                    case 9:
                        this.State = TransmissionState.WrongValue;
                        break;

                    case 10:
                        this.State = TransmissionState.NoAnswer;
                        break;

                    case 11:
                        this.State = TransmissionState.NoFWFile;
                        break;

                    default:
                        this.State = TransmissionState.Error;
                        break;
                }
            }
            if (this.File)
            {
                this.DataLength = 128;
                this.buffer = new byte[137];
            }
            else
            {
                this.DataLength = 4;
                this.buffer = new byte[13];
            }
            Array.Copy(data, startIndex, this.buffer, 0, this.buffer.Length);
        }

        public static byte[] CreateBuffer(ushort receiver, ushort sender, ushort parameter, bool write, bool read, bool master, uint value)
        {
            return ProtocolFrame.CreateBuffer(receiver, sender, parameter, write, read, master, false, false, false, ProtocolFrame.ToByte(value));
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(this.Time.ToString("yyyy-MM-dd HH:mm:ss.fff"), 85);
            stringBuilder.Append(";\t");
            stringBuilder.Append(this.State.ToString());
            stringBuilder.Append(";\t");
            stringBuilder.Append(this.Sender);
            stringBuilder.Append(" -> ");
            stringBuilder.Append(this.Receiver);
            stringBuilder.Append(";\tBits: ");
            if (this.Write)
            {
                stringBuilder.Append('W');
            }
            if (this.Read)
            {
                stringBuilder.Append('R');
            }
            if (this.Master)
            {
                stringBuilder.Append('M');
            }
            if (this.Error)
            {
                stringBuilder.Append('E');
            }
            if (this.File)
            {
                stringBuilder.Append('F');
            }
            if (this.Vdew)
            {
                stringBuilder.Append('V');
            }
            if (!this.Write && !this.Read && !this.Master && !this.Error && !this.File && !this.Vdew)
            {
                stringBuilder.Append('-');
            }
            stringBuilder.Append(";\tParam: ");
            stringBuilder.Append(this.Parameter);
            stringBuilder.Append('=');
            if (this.DataLength == 128)
            {
                stringBuilder.Append("... (").Append(128).Append(" byte)");
            }
            else
            {
                stringBuilder.Append(this.ParameterValue);
            }
            stringBuilder.Append(";\tChecksum: ");
            stringBuilder.Append(this.Checksum);
            return stringBuilder.ToString();
        }

        public static byte[] CreateBuffer(ushort receiver, ushort sender, ushort parameter, bool write, bool read, bool master, bool error, bool file, bool vdew, byte[] value)
        {
            int num = value.Length;
            if (num != 4 && num != 128)
            {
                throw new ArgumentOutOfRangeException("value", string.Concat(new object[]
                    {
                        "value must have ",
                        4,
                        " or ",
                        128,
                        " bytes"
                    }));
            }
            if (parameter > 1023)
            {
                throw new ArgumentOutOfRangeException("parameter", parameter, "only 10 bits (0 - 1023) allowed");
            }
            byte[] array = new byte[9 + value.Length];
            ushort num2 = parameter;
            if (write)
            {
                num2 += 32768;
            }
            if (read)
            {
                num2 += 16384;
            }
            if (master)
            {
                num2 += 8192;
            }
            if (error)
            {
                num2 += 4096;
            }
            if (file)
            {
                num2 += 2048;
            }
            if (vdew)
            {
                num2 += 1024;
            }
            array[0] = 2;
            array[1] = (byte)(receiver >> 8);
            array[2] = (byte)receiver;
            array[3] = (byte)(sender >> 8);
            array[4] = (byte)sender;
            array[5] = (byte)(num2 >> 8);
            array[6] = (byte)num2;
            Array.Copy(value, 0, array, 7, num);
            array[7 + num] = GlobalData.CalcChecksum(array, 1, 6 + num);
            array[8 + num] = 3;
            return array;
        }

        private static byte[] ToByte(uint value)
        {
            return new byte[]
                {
                    (byte)(value >> 24),
                    (byte)(value >> 16),
                    (byte)(value >> 8),
                    (byte)value
                };
        }
    }
}