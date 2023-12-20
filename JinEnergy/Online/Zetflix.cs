﻿using JinEnergy.Engine;
using Microsoft.JSInterop;
using Shared.Engine.Online;

namespace JinEnergy.Online
{
    public class ZetflixController : BaseController
    {
        #region ZetflixController
        static bool origstream;

        static long lastcheckid;
        #endregion

        [JSInvokable("lite/zetflix")]
        async public static ValueTask<string> Index(string args)
        {
            var init = AppInit.Zetflix;
            bool userapn = IsApnIncluded(init);

            var arg = defaultArgs(args);
            int s = int.Parse(parse_arg("s", args) ?? "-1");
            string? t = parse_arg("t", args);

            var oninvk = new ZetflixInvoke
            (
               null,
               init.corsHost(),
               init.hls,
               (url, head) => JsHttpClient.Get(init.cors(url), addHeaders: head),
               streamfile => userapn ? HostStreamProxy(init, streamfile) : DefaultStreamProxy(streamfile, origstream)
               //AppInit.log
            );

            var content = await InvokeCache(arg.id, $"zetfix:view:{arg.kinopoisk_id}:{s}", () => oninvk.Embed(arg.kinopoisk_id, s));
            if (content?.pl == null)
                return EmptyError("content");

            int number_of_seasons = 1;
            if (!content.movie && s == -1 && arg.id > 0)
                number_of_seasons = await InvStructCache(arg.id, $"zetfix:number_of_seasons:{arg.kinopoisk_id}", () => oninvk.number_of_seasons(arg.id));

            if (!userapn && AppInit.Country == "UA" && lastcheckid != arg.kinopoisk_id)
            {
                string? uri = oninvk.FirstLink(content, t, s);
                if (!string.IsNullOrEmpty(uri))
                {
                    lastcheckid = arg.kinopoisk_id;
                    origstream = await IsOrigStream(uri);
                }
            }

            return oninvk.Html(content, number_of_seasons, arg.kinopoisk_id, arg.title, arg.original_title, t, s);
        }
    }
}
