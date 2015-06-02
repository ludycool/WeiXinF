using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Text;
using System.Xml;
using MessageHandle.mode;

namespace MessageHandle
{
    /// <summary>
    /// 接受/发送消息帮助类
    /// </summary>
    public class messageHelp
    {

        string ServerId; //消息接收方微信号，一般为公众平台账号微信号
        string ClientId;//消息发送方微信号
        string MsgId;//接收的消息Id
        string MsgType;//消息类型
        string CreateTime;//消息创建时间


        //返回消息
        public string ReturnMessage(string postStr)
        {

            string responseContent = "";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(new System.IO.MemoryStream(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(postStr)));
            XmlNode xmlMsgType = xmldoc.SelectSingleNode("/xml/MsgType");
            XmlNode ToUserName = xmldoc.SelectSingleNode("/xml/ToUserName");
            XmlNode FromUserName = xmldoc.SelectSingleNode("/xml/FromUserName");
            ServerId = ToUserName.InnerText;
            ClientId = FromUserName.InnerText;
            MsgType = xmlMsgType.InnerText;

            if (MsgType != null)
            {
                switch (MsgType)
                {
                    case "event"://事件处理
                        responseContent = EventHandle(xmldoc);
                        break;
                    case "text"://接受文本消息处理
                        string Content = xmldoc.SelectSingleNode("/xml/Content").InnerText;
                        responseContent = TextHandle(Content);
                        break;


                    case "image"://图像

                        break;

                    case "voice"://语音

                        break;

                    case "video"://视频

                        break;
                    case "location"://位置

                        break;
                    default:
                        break;
                }
            }
            return responseContent;
        }
        //事件
        public string EventHandle(XmlDocument xmldoc)
        {
            string responseContent = "";
            XmlNode Event = xmldoc.SelectSingleNode("/xml/Event");
            XmlNode EventKey = xmldoc.SelectSingleNode("/xml/EventKey");
            XmlNode ToUserName = xmldoc.SelectSingleNode("/xml/ToUserName");
            XmlNode FromUserName = xmldoc.SelectSingleNode("/xml/FromUserName");
            if (Event != null)
            {
                switch (Event.InnerText)
                {
                    case "CLICK"://点击
                        #region


                        switch (EventKey.InnerText)
                        {
                            case "click_one":
                                responseContent = string.Format(ReplyType.Message_Text,
                           FromUserName.InnerText,
                           ToUserName.InnerText,
                           DateTime.Now.Ticks,
                           "你点击的是click_one");

                                break;
                            case "click_two":
                                responseContent = string.Format(ReplyType.Message_News_Main,
                           FromUserName.InnerText,
                           ToUserName.InnerText,
                           DateTime.Now.Ticks,
                           "2",
                            string.Format(ReplyType.Message_News_Item, "我要寄件", "",
                            "http://www.soso.com/orderPlace.jpg",
                            "http://www.soso.com/") +
                            string.Format(ReplyType.Message_News_Item, "订单管理", "",
                            "http://www.soso.com/orderManage.jpg",
                            "http://www.soso.com/"));

                                break;
                            case "click_three":
                                responseContent = string.Format(ReplyType.Message_News_Main,
                           FromUserName.InnerText,
                           ToUserName.InnerText,
                           DateTime.Now.Ticks,
                           "1",
                            string.Format(ReplyType.Message_News_Item, "标题", "摘要",
                            "http://www.soso.com/jieshao.jpg",
                            "http://www.soso.com/"));

                                break;
                        }

                        #endregion
                        break;
                    case "subscribe"://关注

                        break;
                    case "unsubscribe"://取消关注


                        break;

                }

            }
            return responseContent;
        }
        //接受文本消息
        public string TextHandle(string Content)
        {
            string responseContent = SendText("欢迎使用微信公共账号，您输入的内容为：" + Content + "\r\n<a href=\"http://www.baidu.com\">点击进入</a>");
            return responseContent;
        }


        //返回文本消息
        public string SendText(string Content)
        {
            string responseContent = "";
            if (Content != null)
            {
                responseContent = string.Format(ReplyType.Message_Text, ClientId, ServerId, DateTime.Now.Ticks, Content);
            }
            return responseContent;
        }
        //返回图文
        public string SendMessage_News(List<NewsItem> listnews)
        {
            string Item = "";
            for (int i = 0; i < listnews.Count; i++)
            {

                Item += string.Format(ReplyType.Message_News_Item, listnews[0].Title, listnews[0].Description, listnews[0].PicUrl, listnews[0].Url);
            }
            string responseContent = "";
            responseContent = string.Format(ReplyType.Message_News_Main,
                          ClientId,
                          ServerId,
                           DateTime.Now.Ticks,
                          listnews.Count,
                          Item);
            return responseContent;
        }
        //写入日志 
        public void WriteLog(string text)
        {
            StreamWriter sw = new StreamWriter(HttpContext.Current.Server.MapPath(".") + "\\log.txt", true);
            sw.WriteLine(text);
            sw.Close();//写入
        }
        //  System.Net.WebClient web = new System.Net.WebClient();
        //  string result = web.DownloadString(url);       
    }

    //回复类型格式
    public class ReplyType
    {
        /// <summary>
        /// 普通文本消息
        /// </summary>
        public static string Message_Text
        {
            get
            {
                return @"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[text]]></MsgType>
                            <Content><![CDATA[{3}]]></Content>
                            </xml>";
            }
        }
        /// <summary>
        /// 图文消息主体
        /// </summary>
        public static string Message_News_Main
        {
            get
            {
                return @"<xml>
                            <ToUserName><![CDATA[{0}]]></ToUserName>
                            <FromUserName><![CDATA[{1}]]></FromUserName>
                            <CreateTime>{2}</CreateTime>
                            <MsgType><![CDATA[news]]></MsgType>
                            <ArticleCount>{3}</ArticleCount>
                            <Articles>
                            {4}
                            </Articles>
                            </xml> ";
            }
        }
        /// <summary>
        /// 图文消息项
        /// </summary>
        public static string Message_News_Item
        {
            get
            {
                return @"<item>
                            <Title><![CDATA[{0}]]></Title> 
                            <Description><![CDATA[{1}]]></Description>
                            <PicUrl><![CDATA[{2}]]></PicUrl>
                            <Url><![CDATA[{3}]]></Url>
                            </item>";
            }
        }


        /// <summary>
        /// 音乐
        /// </summary>
        public static string Message_Music
        {

            get
            {

                return @"<xml>
    <ToUserName><![CDATA[{0}]]></ToUserName>
    <FromUserName><![CDATA[{1}]]></FromUserName>
    <CreateTime>{2}</CreateTime>
    <MsgType><![CDATA[music]]></MsgType>
    <Music>
        <Title><![CDATA[{3}]]></Title>
        <Description><![CDATA[{4}]]></Description>
        <MusicUrl><![CDATA[{5}]]></MusicUrl>
        <HQMusicUrl><![CDATA[{6}]]></HQMusicUrl>
    </Music>
    <FuncFlag>0</FuncFlag>
    </xml>";

            }

        }
    }
}