using System;
using System.Web;
using System.Linq;

using ShoppingPeeker.Utilities;
//using ShoppingPeeker.DbManage;
//using ShoppingPeeker.DomainEntity;
using ShoppingPeeker.Utilities.DEncrypt;
using ShoppingPeeker.Utilities.Interface;
using Microsoft.AspNetCore.Http;

/*
 �����������֤
 ģ�����֤�ģ���֤��¼��Cookie
 */
namespace ShoppingPeeker.Web.Authentication
{
    /// <summary>
    /// Authentication service
    /// </summary>
    public  class FormsAuthenticationService
    {
        #region  �ֶ�
        /// <summary>
        /// Ĭ��5����  ��¼��ʱ��������ʱ��
        /// </summary>
        private readonly TimeSpan _expirationTimeSpan = TimeSpan.FromMinutes(5);

        /// <summary>
        /// ����/���ܷ���
        /// </summary>
        private readonly EncryptionService _encryptionService = new EncryptionService();
        #endregion




        /// <summary>
        /// Ctor
        /// </summary>
        public FormsAuthenticationService()
        {
            //��ʼ��  ��¼��ʱ����
            var authConfig = ConfigHelper.HostingConfiguration.GetConfig("AuthTimeout");// FormsAuthentication.Timeout;
            if (!authConfig.IsNullOrEmpty())
            {
                double tim;
                double.TryParse(authConfig, out tim);
                if (tim > 0)
                {
                    this._expirationTimeSpan = TimeSpan.FromMinutes(tim);

                }

            }
        }

        /// <summary>
        /// ��¼ʹ�õı�+Cookie����֤
        /// ��֤ͨ���� ���ڿͻ������ɴ�����֤����ݱ�ʶ��Cookie������һ�λط�����������ʱ��
        /// �Ὣ��ݱ�ʶ��������������
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isRememberLogin"></param>
        public virtual bool SignIn(string userName, string pwd)
        {
            var result = false;

            //UserInfoModel user = null;

            ////---------Todo:ͨ����ͳһ���������ȡ�û���Ϣ
            ////user = dal_Users
            ////      .GetElementsByCondition(x => x.UserName==userName)
            ////      .FirstOrDefault();

            //if (null == user)
            //{
            //    return result;
            //}

            ////2 ��ѯ�����û���  �Աȼ��ܹ�����Ȩ��Ϣ��license key
            ////var encryPwd = EncryptionService.CreatePasswordHash(pwd, user.PasswordSalt);
            ////if (!string.Equals(encryPwd,user.Password))
            ////{
            ////    return result;
            ////}
            ////else
            ////{
            ////    result = true;
            ////}


            ////3 ��֤ͨ����  ��ȨCookie�Ĵ���  ����Ʊ�� ������
            //var tokenData = user.ToJson().KeyEncrypt();
            //var ticket = this._encryptionService.GenerateFormsAuthenticationTicket(tokenData, this._expirationTimeSpan);
            //var encryptedTicket = this._encryptionService.EncryptFormsAuthenticationTicket(ticket);

            ////var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            ////cookie.HttpOnly = true;
            ////if (ticket.IsPersistent)
            ////{
            ////    cookie.Expires = ticket.Expiration;
            ////}
            //////cookie.Secure = FormsAuthentication.RequireSSL;
            ////cookie.Path = FormsAuthentication.FormsCookiePath;
            ////if (FormsAuthentication.CookieDomain != null)
            ////{
            ////    cookie.Domain = FormsAuthentication.CookieDomain;
            ////}

            //HttpContext.Current.SetCookie<string>(FormsAuthentication.CookieDomain,
            //    Contanst.Login_Cookie_Client_Key,
            //    encryptedTicket
            //    );


            result = true;
            return result;
        }

        /// <summary>
        /// ����ͻ��˵ĵ�¼ƾ��
        /// </summary>
        public virtual void SignOut()
        {
            WorkContext.HttpContext.RemoveCookie(Contanst.Global_Site_Domain_Cookie, Contanst.Login_Cookie_Client_Key);
        }

        ///// <summary>
        ///// ����֤����Cookie�л�ȡ��¼�û���Ϣ
        ///// </summary>
        ///// <returns></returns>
        //public virtual UserInfoModel GetAuthenticatedCustomerFromCookie()
        //{

        //    try
        //    {


        //        //����
        //        var encryptedTicket = HttpContext.Current.GetCookie<string>(Contanst.Login_Cookie_Client_Key);// this._encryptionService.EncryptFormsAuthenticationTicket(ticket);
        //        if (encryptedTicket.IsNullOrEmpty())
        //        {
        //            return null;
        //        }
        //        //�õ�ƾ��
        //        var ticket = this._encryptionService.DecryptFormsAuthenticationTicket(encryptedTicket);
        //        if (null == ticket || string.IsNullOrEmpty(ticket.UserData))
        //        {
        //            return null;
        //        }

        //        //����ƾ���е�����

        //        var tokenData = ticket.UserData.KeyDecrypt();

        //        if (!tokenData.IsNullOrEmpty())
        //        {
        //            tokenData = tokenData.KeyDecrypt();//��������
        //        }


        //        return tokenData.FromJson<UserInfoModel>();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }


        //}

    }
}