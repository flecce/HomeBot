namespace CommonHelpers.Inverters.Enums
{
    public struct CommonStatus
    {
        public readonly ushort Status;
        public readonly ushort Status1;
        public readonly ushort Status2;
        public readonly float EnergieTag;
        public readonly float EnergieWoche;
        public readonly float EnergieMonat;
        public readonly float EnergieJahr;
        public readonly float EnergieGesamt;
        public readonly byte LetzterFehler;

        public CommonStatus(ushort status, ushort status1, ushort status2, float energieTag, float energieWoche, float energieMonat, float energieJahr, float energieGesamt, byte letzterFehler)
        {
            this.Status = status;
            this.Status1 = status1;
            this.Status2 = status2;
            this.EnergieTag = energieTag;
            this.EnergieWoche = energieWoche;
            this.EnergieMonat = energieMonat;
            this.EnergieJahr = energieJahr;
            this.EnergieGesamt = energieGesamt;
            this.LetzterFehler = letzterFehler;
        }
    }
}