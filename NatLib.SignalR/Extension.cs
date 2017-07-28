using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace NatLib.SignalR
{
    public static class Extension
    {
        /// <summary>
        /// Returns the <see cref="HttpContextBase"/> for this <see cref="IRequest"/>.
        /// </summary>
        /// <param name="request">The request</param>
/*
        public static HttpContextBase GetHttpContext(this IRequest request)
        {
            object httpContextBaseValue;
            if (request.Environment.TryGetValue(typeof(HttpContextBase).FullName, out httpContextBaseValue))
            {
                return httpContextBaseValue as HttpContextBase;
            }
            return null;
        }
*/
        public static string GetRemoteIpAddress(this IRequest request)
        {
            object ipAddress;
            //server.LocalIpAddress
            if (request.Environment.TryGetValue("server.RemoteIpAddress", out ipAddress))
            {
                return ipAddress as string;
            }
            return "";
        }

    }
}
