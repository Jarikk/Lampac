﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Lampac.Engine.Middlewares
{
    public class Accsdb
    {
        private readonly RequestDelegate _next;
        IMemoryCache memoryCache;

        public Accsdb(RequestDelegate next, IMemoryCache mem)
        {
            _next = next;
            memoryCache = mem;
        }

        public Task Invoke(HttpContext httpContext)
        {
            string jacpattern = "^/(api/v2.0/indexers|api/v1.0/|toloka|rutracker|rutor|torrentby|nnmclub|kinozal|bitru|selezen|megapeer|animelayer|anilibria|anifilm|toloka|lostfilm|baibako|hdrezka)";

            if (!string.IsNullOrWhiteSpace(AppInit.conf.jac.apikey))
            {
                if (Regex.IsMatch(httpContext.Request.Path.Value, jacpattern))
                {
                    if (AppInit.conf.jac.apikey != Regex.Match(httpContext.Request.QueryString.Value, "(\\?|&)apikey=([^&]+)").Groups[2].Value)
                        return Task.CompletedTask;
                }
            }

            if (AppInit.conf.accsdb.enable)
            {
                if (httpContext.Request.Path.Value != "/" && !Regex.IsMatch(httpContext.Request.Path.Value, jacpattern) && 
                    !Regex.IsMatch(httpContext.Request.Path.Value, "^/(((b2pay|cryptocloud|freekassa|litecoin)/)|lite/(filmixpro|kinopubpro|vokinotk)|lampa-(main|lite)/app\\.min\\.js|[a-zA-Z]+\\.js|msx/start\\.json|samsung\\.wgt)"))
                {
                    bool limitip = false;
                    HashSet<string> ips = null;
                    string account_email = HttpUtility.UrlDecode(Regex.Match(httpContext.Request.QueryString.Value, "(\\?|&)account_email=([^&]+)").Groups[2].Value)?.ToLower();

                    if (string.IsNullOrWhiteSpace(account_email) || AppInit.conf.accsdb.accounts.FirstOrDefault(i => i.ToLower() == account_email) == null || 
                        IsLockHostOrUser(account_email, httpContext.Connection.RemoteIpAddress.ToString(), out limitip, out ips))
                    {
                        if (Regex.IsMatch(httpContext.Request.Path.Value, "\\.(js|css|ico|png|svg|jpe?g|woff|webmanifest)"))
                        {
                            httpContext.Response.StatusCode = 404;
                            httpContext.Response.ContentType = "application/octet-stream";
                            return Task.CompletedTask;
                        }

                        string msg = string.IsNullOrWhiteSpace(account_email) ? AppInit.conf.accsdb.cubMesage : AppInit.conf.accsdb.denyMesage.Replace("{account_email}", account_email);
                        if (limitip)
                            msg = $"Превышено допустимое количество ip. Разбан через {60 - DateTime.Now.Minute} мин.\n{string.Join(", ", ips)}";

                        httpContext.Response.ContentType = "application/javascript; charset=utf-8";
                        return httpContext.Response.WriteAsync("{\"accsdb\":true,\"msg\":\"" + msg + "\"}");
                    }
                }
            }

            return _next(httpContext);
        }



        bool IsLockHostOrUser(string account_email, string userip, out bool islock, out HashSet<string> ips)
        {
            string memKeyLocIP = $"Accsdb:IsLockHostOrUser:{account_email}:{DateTime.Now.Hour}";

            if (memoryCache.TryGetValue(memKeyLocIP, out ips))
            {
                ips.Add(userip);
                memoryCache.Set(memKeyLocIP, ips, DateTime.Now.AddHours(1));

                if (ips.Count > AppInit.conf.accsdb.maxiptohour)
                {
                    islock = true;
                    return islock;
                }
            }
            else
            {
                ips = new HashSet<string>() { userip };
                memoryCache.Set(memKeyLocIP, ips, DateTime.Now.AddHours(1));
            }

            islock = false;
            return islock;
        }
    }
}