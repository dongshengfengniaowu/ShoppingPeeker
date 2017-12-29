﻿using System;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using ShoppingPeeker.Utilities;
using ShoppingPeeker.Utilities.Ioc;
using ShoppingPeeker.Utilities.Logging;
using ShoppingPeeker.Utilities.DEncrypt;
using Microsoft.Extensions.Configuration;
using ShoppingPeeker.Web.ViewModels;
using NTCPMessage.EntityPackage.Arguments;
using ShoppingPeeker.Utilities.Caching;

namespace ShoppingPeeker.Web
{
    /// <summary>
    /// Work context for web application
    /// </summary>
    public class WorkContext
    {


        #region EventHandlers

        /// <summary>
        /// 当站点承载配置文件发生变更的时候触发的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ConfigHelper_OnHostingConfigChangedEvent(object sender, ConfigChangedEventArgs e)
        {
            if (null == e || null == e.ResultOfChangedConfig)
            {
                return;
            }
            IConfiguration config = e.ResultOfChangedConfig;
            //设置刷新站点名称
            string siteName = config.GetConfig(Contanst.Config_Node_SiteName);
            if (!string.IsNullOrEmpty(siteName))
            {
                SiteName = siteName;
            }
            //设置请求戳的过期验证时间
            int signTimeOut = config.GetConfigInt(Contanst.Config_Node_SignTimeOut);
            SignTimeOut = signTimeOut;
        }

        #endregion

        #region Properties


        private static string _SiteName;

        /// <summary>
        /// /站点名称
        /// </summary>
        public static string SiteName
        {
            get
            {

                if (string.IsNullOrEmpty(_SiteName))
                {
                    _SiteName = ConfigHelper.HostingConfiguration.GetConfig(Contanst.Config_Node_SiteName);
                }
                if (string.IsNullOrEmpty(_SiteName))
                {
                    _SiteName = Contanst.Default_Site_Domain_Name;
                }
                return _SiteName;

            }
            private set
            {
                _SiteName = value;
            }

        }

        private static int _signTimeOut = ConfigHelper.HostingConfiguration.GetConfigInt(Contanst.Config_Node_SignTimeOut);
        /// <summary>
        /// 请求标识的失效时间（秒）
        /// </summary>
        public static int SignTimeOut
        {
            get
            {
                if (_signTimeOut <= 0)
                {
                    _signTimeOut = Contanst.Default_SignTimeOut;
                }
                return _signTimeOut;
            }
            private set
            {
                _signTimeOut = value;
            }
        }
        /// <summary>
        /// 是否开启抓取页面的缓存
        /// </summary>
        public static bool IsFetchPageCacheaAble
        {
            get
            {
                var isAble = ConfigHelper.HostingConfiguration.GetConfigBool(Contanst.Config_Node_IsFetchPageCacheaAble);
                return isAble;
            }
        }
        public static IHostingEnvironment HostingEnvironment { get; set; }

        ///// <summary>
        ///// 蜘蛛ip地址
        ///// </summary>
        //public static string ShoppingWebCrawlerAddress {
        //    get {

        //        var addr= ConfigHelper.AppSettingsConfiguration.GetConfig("ShoppingWebCrawlerAddress");
        //        if (string.IsNullOrEmpty(addr))
        //        {
        //            addr= "127.0.0.1";
        //        }
        //        return addr;
        //    } }
        ///// <summary>
        ///// 蜘蛛端口
        ///// </summary>
        //public static int ShoppingWebCrawlerPort
        //{
        //    get
        //    {
        //        var port = ConfigHelper.AppSettingsConfiguration.GetConfigInt("ShoppingWebCrawlerPort");
        //        return port;
        //    }
        //}


        /// <summary>
        /// 暴露 当前的Http 上下文
        /// </summary>
        public static HttpContext HttpContext
        {
            get
            {
                object factory = NativeIocContainer.GetInstance<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
                if (null == factory)
                {
                    throw new Exception("未能找到 IHttpContextAccessor 的实例");
                }
                Microsoft.AspNetCore.Http.HttpContext context = ((Microsoft.AspNetCore.Http.HttpContextAccessor)factory).HttpContext;

                return context;
            }

        }

        /// <summary>
        /// 错误页面地址
        /// </summary>
        public static string ErrorPage
        {
            get
            {
                return "/home/error";
            }
        }


        ///// <summary>
        ///// 根据用户回发过来的Cookie进行用户的查询
        ///// </summary>
        //public virtual Users CurrentUser
        //{
        //    get
        //    {

        //        ///未经过登录验证的 返回空
        //        if (_httpContext == null ||
        //       _httpContext.Request == null ||
        //      _httpContext.User == null)
        //        {
        //            return null;
        //        }

        //        if (_cachedUser != null)
        //            return _cachedUser;

        //        Users user = null;



        //        //----从用户回发过来的Cookie解析用户数据

        //        var customerCookie = GetCustomerCookie();
        //        var ticket = this.encryptionService.DecryptFormsAuthenticationTicket(customerCookie.Value);

        //        if (ticket != null && !String.IsNullOrEmpty(ticket.UserData))
        //        {
        //            Guid customerGuid;
        //            if (Guid.TryParse(ticket.UserData, out customerGuid))
        //            {
        //                var customerByCookie = dal_Users
        //                    .GetElementsByCondition(x => x.UserGuid == customerGuid)
        //                    .FirstOrDefault();
        //                user = customerByCookie;
        //            }
        //        }
        //        if (null != user)
        //        {
        //            _cachedUser = user;
        //        }

        //        return _cachedUser;
        //    }
        //    set
        //    {
        //        //将用户身份标识  GUID写入到客户端的Cookie
        //        SetCustomerCookie(value.UserGuid);
        //        _cachedUser = value;
        //    }
        //}

        ///private bool _IsAdmin;
        /// <summary>
        /// 是否是管理员
        /// </summary>
        //public bool IsAdmin
        //{
        //    get
        //    {
        //        if (null != CurrentUser && CurrentUser.IsHasActived && !CurrentUser.IsDeleted)
        //        {
        //            this._IsAdmin = true;
        //        }

        //        return _IsAdmin;
        //    }

        //}
        /// <summary>
        /// 当前用户的登录请求IP地址
        /// </summary>
        public static string CurrentUserIpAddress
        {
            get
            {
                string userIP = HttpContext.Request.GetIP();
                return userIP;
            }
        }




        public static bool IsAdmin
        {
            get
            {
                throw new NotImplementedException();
            }
        }



        private static RedisCacheManager _RedisClient;
        /// <summary>
        /// redis 客户端
        /// </summary>
        public static RedisCacheManager RedisClient
        {
            get
            {
                if (null == _RedisClient)
                {
                    _RedisClient = RedisCacheManager.Current;
                }
                return _RedisClient;
            }
            set
            {
                _RedisClient = value;
            }
        }


        #endregion


        #region Utilities


        /// <summary>
        /// 从缓存中读取抓取页面的结果
        /// </summary>
        /// <param name="webArgs"></param>
        /// <returns></returns>
        public static SearchProductViewModel GetFetchPageResultFromCache(BaseFetchWebPageArgument webArgs)
        {
            SearchProductViewModel reultModel = null;
            var key = webArgs.CacheKey;
            reultModel = RedisClient.Get< SearchProductViewModel>(key);
            return reultModel;
        }

        /// <summary>
        ///  将指定的参数的抓取的页面的解析结果放到缓存
        /// </summary>
        /// <param name="webArgs"></param>
        /// <param name="reultModel"></param>
        /// <param name="timeOut">默认为30秒</param>
        public static void SetFetchPageResultFromCache(BaseFetchWebPageArgument webArgs, SearchProductViewModel reultModel, int timeOut = 30)
        {
            var key = webArgs.CacheKey;
            RedisClient.SetAsync(key, reultModel, timeOut);
        }

        /// <summary>
        /// 直接重定向请求 到错误页面
        /// </summary>
        public static void GoToErrorPage()
        {
            var context = HttpContext;
            context.Response.Clear();
            context.Response.Redirect(WorkContext.ErrorPage);
        }

        /// <summary>
        /// 验证是否是合法的请求标识
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsValidRequestSign(string sign)
        {
            bool result = false;
            if (string.IsNullOrEmpty(sign))
            {
                return false;
            }
            try
            {
                //1 先解密
                string deCodeString = LZString.Decompress(sign, true);
                //2 获取里面的时间戳内容对应的时间
                DateTime clientTimeSign = deCodeString.ToLong().ConvertUnixTimeTokenToDateTime();
                //3 比较时间是否超出 2分钟 超过2分钟的请求sign 标志为失效的请求
                if (DateTime.Now.Subtract(clientTimeSign).TotalSeconds <= SignTimeOut)
                {
                    result = true;
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return result;
        }



        //protected virtual HttpCookie GetCustomerCookie()
        //{
        //    if (_httpContext == null || _httpContext.Request == null)
        //        return null;
        //    var authKey = _httpContext.Request.Cookies.AllKeys.FirstOrDefault(x => x.Contains(FormsAuthentication.FormsCookieName));
        //    return _httpContext.Request.Cookies[authKey];
        //}

        ///// <summary>
        ///// 将用户的GUID写入到客户端
        ///// </summary>
        ///// <param name="customerGuid"></param>
        //protected virtual void SetCustomerCookie(Guid customerGuid)
        //{
        //    if (_httpContext != null && _httpContext.Response != null)
        //    {
        //        var cookie = new HttpCookie(CustomerCookieName);
        //        cookie.HttpOnly = true;
        //        cookie.Value = customerGuid.ToString();
        //        if (customerGuid == Guid.Empty)
        //        {
        //            cookie.Expires = DateTime.Now.AddMonths(-1);
        //        }
        //        else
        //        {
        //            int cookieExpires = 24 * 365; //TODO make configurable
        //            cookie.Expires = DateTime.Now.AddHours(cookieExpires);
        //        }

        //        _httpContext.Response.Cookies.Remove(CustomerCookieName);
        //        _httpContext.Response.Cookies.Add(cookie);
        //    }
        //}


        #endregion





    }
}
