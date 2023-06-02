﻿using Lampac.Models.LITE.Kodik;

namespace Shared.Model.Online.Kodik
{
    public class Result
    {
        public string id { get; set; }

        public string type { get; set; }

        public string link { get; set; }

        public Translation translation { get; set; }

        public int last_season { get; set; }

        public Dictionary<string, Season> seasons { get; set; }
    }
}
