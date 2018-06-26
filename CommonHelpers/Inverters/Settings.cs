namespace CommonHelpers.Inverters
{
    public sealed class Settings
    {
        public sealed class Default
        {
            public static int ReadFrameTcpTimeout
            { get { return 10000; } set { } }

            public static int ReadFrameCancelTimeout
            { get { return 500; } set { } }

            public static int ReadFileLengthRepeat
            { get { return 15; } set { } }

            public static int ReadFileLengthTimeout
            { get { return 90000; } set { } }

            public static int ReadFileBlockRepeat
            { get { return 4; } set { } }

            public static int WriteParameterRepeat
            { get { return 3; } set { } }
        }
    }
}