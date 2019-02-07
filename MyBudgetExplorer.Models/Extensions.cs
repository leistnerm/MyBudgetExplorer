using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyBudgetExplorer.Models
{
    public static class Extensions
    {
        #region Public Methods
        public static string ToDisplay(this long d)
        {
            return (d / 1000M).ToString("N2");
        }

        public static string ToHtmlTable(this Exception e)
        {
            if (e == null)
                return string.Empty;

            var ex = e;
            var sb = new StringBuilder();
            do
            {
                sb.Append("<table border=\"1\">");
                sb.Append($"<tr><th colspan=\"2\">{ex.GetType().Name}</th></tr>");
                sb.Append($"<tr><td>HResult</td><td>{ex.HResult}</td></tr>");
                sb.Append($"<tr><td>Message</td><td>{ex.Message}</td></tr>");
                sb.Append($"<tr><td>Source</td><td>{ex.Source}</td></tr>");
                sb.Append($"<tr><td>Target Site</td><td>{ex.TargetSite}</td></tr>");
                sb.Append($"<tr><td>Stack Trace</td><td><pre>{ex.StackTrace}</pre></td></tr>");
                if (ex.Data.Count > 0)
                    foreach (var key in ex.Data.Keys)
                        sb.Append($"<tr><td>Data: {key}</td><td><pre>{ex.Data[key]}</pre></td></tr>");
                sb.Append("</table><hr />");

                ex = ex.InnerException;
            } while (ex != null);

            return sb.ToString();
        }

        public static string ToHtmlTable(this HttpRequest r)
        {
            if (r == null)
                return string.Empty;

            var sb = new StringBuilder();

            sb.Append("<table border=\"1\">");
            sb.Append($"<tr><td>Path</td><td>{r.Path}</td></tr>");
            sb.Append($"<tr><td>Query String</td><td>{r.QueryString}</td></tr>");
            if (r.HttpContext != null)
            {
                sb.Append($"<tr><td>IP</td><td>{r.HttpContext.Connection.RemoteIpAddress}</td></tr>");
                sb.Append($"<tr><td>Authenticated</td><td>{r.HttpContext.User.Identity.IsAuthenticated}</td></tr>");
            }
            foreach (var header in r.Headers)
                sb.Append($"<tr><td>{header.Key}</td><td>{header.Value}</td></tr>");
            sb.Append("</table><hr />");

            return sb.ToString();
        }
        #endregion
    }
}
