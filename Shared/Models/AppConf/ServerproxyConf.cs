﻿using Shared.Model.Base;
using Shared.Models.AppConf;

namespace Lampac.Models.AppConf
{
    public class ServerproxyConf : Iproxy
    {
        public bool enable { get; set; }

        public bool encrypt { get; set; }

        public bool verifyip { get; set; }

        public bool allow_tmdb { get; set; }

        public bool showOrigUri { get; set; }

        public ServerproxyCacheConf cache { get; set; } = new ServerproxyCacheConf();


        public bool useproxy { get; set; }

        public bool useproxystream { get; set; }

        public string globalnameproxy { get; set; }

        public ProxySettings? proxy { get; set; }
    }
}
