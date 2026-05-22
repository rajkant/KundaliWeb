namespace KundaliWeb.Models
{
    public class cpd
    {
        public cpd Clone()
        {
            return new cpd()
            {
                etut = this.etut,
                lon_e_w = this.lon_e_w,
                lat_n_s = this.lat_n_s,
                ephe = this.ephe,
                plansel = this.plansel,
                ctr = this.ctr,
                hsysname = this.hsysname,
                sast = this.sast,
                mday = this.mday,
                mon = this.mon,
                hour = this.hour,
                min = this.min,
                sec = this.sec,
                year = this.year,
                lon_deg = this.lon_deg,
                lon_min = this.lon_min,
                lon_sec = this.lon_sec,
                lat_deg = this.lat_deg,
                lat_min = this.lat_min,
                lat_sec = this.lat_sec,
                alt = this.alt
            };
        }
        public string etut = null;
        public string lon_e_w = null;
        public string lat_n_s = null;
        public string ephe = null;
        public string plansel = null;
        public string ctr = null;
        public string hsysname = null;
        public string sast = null;
        public uint mday = 0, mon = 0, hour = 0, min = 0, sec = 0;
        public int year = 0;
        public uint lon_deg = 0, lon_min = 0, lon_sec = 0;
        public uint lat_deg = 0, lat_min = 0, lat_sec = 0;
        public int alt = 0;
    }
}
