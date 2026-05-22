namespace VedAstro.Library
{
    /// <summary>
    /// Holds info (Dasa, Bhukti & Antaram) on the ruling period at point in time.
    /// Dasas are major periods in which the indications of the planets are realised.
    /// </summary>
    public struct Dasas
    {
        //DATA FIELDS
        public PlanetName PD1 { get; set; }
        public PlanetName PD2 { get; set; }
        public PlanetName PD3 { get; set; }
        public PlanetName PD4 { get; set; }
        public PlanetName PD5 { get; set; }
        public PlanetName PD6 { get; set; }
        public PlanetName PD7 { get; set; }
        public PlanetName PD8 { get; set; }
    }

    public struct Dasas2
    {
        public PlanetName Dasa { get; set; }

        public PlanetName Bhukti { get; set; }

        public PlanetName Antaram { get; set; }

        public PlanetName Sukshma { get; set; }
    }
}


