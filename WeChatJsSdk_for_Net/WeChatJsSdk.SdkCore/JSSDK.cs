///////////////////////////////////////////////////////////
//  JSSDK.cs
//  Implementation of the Class JSSDK
//  Created on:      09-����-2015 15:11:31
//  Original author: nboss
//  �鿴���£�https://github.com/nboss/WeChatJsSdk
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using WeChatJsSdk.Utility;
namespace WeChatJsSdk.SdkCore
{


    public class JSSDK
    {

        private string appId;
        private string appSecret;
        private bool _debug;
        private SimpleCacheProvider _cache;


        const string URL_FORMAT_TOKEN = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
        const string URL_FORMAT_TICKET = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";

        const string CACHE_TOKEN_KEY = "CACHE_TOKEN_KEY";
        const string CACHE_TICKET_KEY = "CACHE_TICKET_KEY";


        /// <summary>
        /// ����JDK����
        /// </summary>
        /// <param name="appId">΢�Ź����˺ŵ�AppId</param>
        /// <param name="appSecret">�����˺ŵ�appSecret</param>
        /// <param name="debug">�Ƿ����</param>
        public JSSDK(string appId, string appSecret, bool debug = false)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this._cache = SimpleCacheProvider.GetInstance();
            this._debug = debug;
        }





        /// <summary>
        /// ����ǩ��������ַ���
        /// </summary>
        /// <param name="length">���ȣ�С��32λ��Ĭ��16λ</param>
        /// <returns>����ַ���</returns>
        private string CreateNonceStr(int length = 16)
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, length > 32 ? 32 : length);
        }
        /// <summary>
        /// ��ȡAccessToken
        /// </summary>
        /// <returns>AccessToken</returns>
        private string GetAccessToken()
        {
            var token = this._cache.GetCache(CACHE_TOKEN_KEY);
            if (token != null)
                return token.ToString();

            try
            {
                string result = HttpGet(string.Format(URL_FORMAT_TOKEN, this.appId, this.appSecret));
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> jsonObj = serializer.Deserialize<dynamic>(result);
                if (jsonObj.ContainsKey("access_token"))
                {
                    token = jsonObj["access_token"].ToString();
                    this._cache.SetCache(CACHE_TOKEN_KEY, token.ToString(), 7000);
                }
                else
                {
                    //Ϊ�˳����������У����׳����󣬿��Լ�¼��־
                    token = jsonObj["errmsg"];
                }
            }
            catch
            {
                //Ϊ�˳����������У����׳����󣬿��Լ�¼��־
                token = "there_is_an_error_when_getting_token";
            }

            return token.ToString();
        }

        /// <summary>
        /// ��ȡApiTicket
        /// </summary>
        /// <returns>ApiTicket</returns>
        private string GetJsApiTicket()
        {
            var ticket = this._cache.GetCache(CACHE_TICKET_KEY);
            if (ticket != null)
                return ticket.ToString();
            try
            {
                string result = HttpGet(string.Format(URL_FORMAT_TICKET, this.GetAccessToken()));
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Dictionary<string, object> jsonObj = serializer.Deserialize<dynamic>(result);
                if (jsonObj.ContainsKey("ticket"))
                {
                    ticket = jsonObj["ticket"].ToString();
                    this._cache.SetCache(CACHE_TICKET_KEY, ticket.ToString(), 7000);
                }
                else
                {
                    //Ϊ�˳����������У����׳����󣬿��Լ�¼��־
                    ticket = jsonObj["errmsg"];
                }
            }
            catch
            {
                //Ϊ�˳����������У����׳����󣬿��Լ�¼��־
                ticket = "there_is_an_error_when_getting_apiticket";
            }

            return ticket.ToString();
        }
        /// <summary>
        /// ��ȡjssdkǩ�����ö���
        /// </summary>
        /// <param name="jsapi">JsApiEnum,��:JsApiEnum.scanQRCode|JsApiEnum.onMenuShareQQ</param>
        /// <returns>΢�Ź���ƽ̨JsSdk�����ö���</returns>
        public SignPackage GetSignPackage(JsApiEnum jsapi)
        {
            HttpContext httpcontext = System.Web.HttpContext.Current;
            string url = (!string.IsNullOrEmpty(httpcontext.Request.ServerVariables["HTTPS"])) && httpcontext.Request.ServerVariables["HTTPS"] != "off" ? "https://" : "http://";
            url += httpcontext.Request.ServerVariables["HTTP_HOST"];
            url += httpcontext.Request.ServerVariables["URL"];
            url += string.IsNullOrEmpty(httpcontext.Request.ServerVariables["QUERY_STRING"]) ? "" : httpcontext.Request.ServerVariables["QUERY_STRING"];
            return GetSignPackage(url, jsapi);
        }
        /// <summary>
        /// ��ȡjssdkǩ�����ö���
        /// </summary>
        /// <param name="url">��ǰҳ��url</param>
        /// <param name="jsapi">JsApiEnum,��:JsApiEnum.scanQRCode|JsApiEnum.onMenuShareQQ</param>
        /// <returns>΢�Ź���ƽ̨JsSdk�����ö���</returns>
        public SignPackage GetSignPackage(string url, JsApiEnum jsapi)
        {

            /*
             * ǩ���ֶ�
            noncestr=Wm3WZYTPz0wzccnW
            jsapi_ticket=sM4AOVdWfPE4DxkXGEs8VMCPGGVi4C3VM0P37wVUCFvkVAy_90u5h9nbSlYy3-Sl-HhTdfl2fzFy1AOcHKP7qg
            timestamp=1414587457
            url=http://mp.weixin.qq.com?params=value  
             */
            string noncestr = this.CreateNonceStr(16);
            string jsapi_tkcket = this.GetJsApiTicket();
            long timestamp = TimeStamp.Now();
            Dictionary<string, string> signData = new Dictionary<string, string>() { 
                {"noncestr",noncestr},
                {"jsapi_ticket",jsapi_tkcket},
                {"timestamp",timestamp.ToString()},
                {"url",url}
            };

            SignPackage result = new SignPackage()
            {
                appId = this.appId,
                timestamp = timestamp,
                nonceStr = noncestr,
                debug = this._debug,
                signature = new Signature().Sign(signData),
                jsApiList = jsapi.ToString().Replace(" ", "").Split(',')
            };
            return result;
        }
        /// <summary>
        /// ��̨����http����
        /// </summary>
        /// <param name="url">����URL</param>
        /// <returns>�������ַ���</returns>
        private string HttpGet(string url)
        {
            return new WebClient().DownloadString(url);
        }

    }
}