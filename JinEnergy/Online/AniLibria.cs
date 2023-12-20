﻿using JinEnergy.Engine;
using Lampac.Models.LITE.AniLibria;
using Microsoft.JSInterop;
using Shared.Engine.Online;

namespace JinEnergy.Online
{
    public class AniLibriaController : BaseController
    {
        [JSInvokable("lite/anilibria")]
        async public static ValueTask<string> Index(string args)
        {
            var init = AppInit.AnilibriaOnline;

            var arg = defaultArgs(args);
            string? code = parse_arg("code", args);

            if (string.IsNullOrEmpty(arg.title))
                return EmptyError("arg");
            
            var oninvk = new AniLibriaInvoke
            (
               null,
               init.corsHost(),
               ongettourl => JsHttpClient.Get<List<RootObject>>(init.cors(ongettourl)),
               streamfile => HostStreamProxy(init, streamfile)
               //AppInit.log
            );

            var content = await InvokeCache(arg.id, $"anilibriaonline:{arg.title}", () => oninvk.Embed(arg.title));
            if (content == null)
            {
                return EmptyError("content");
            }

            return oninvk.Html(content, arg.title, code, arg.year);
        }
    }
}
