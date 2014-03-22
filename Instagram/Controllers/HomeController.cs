using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Instagram.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Auth(string client_id, string client_secret)
        {

            Session["client_id"] = client_id;
            Session["client_secret"] = client_secret;
            var redirect_uri = "http://localhost:53938/home/gettoken";
            var url = string.Format("https://api.instagram.com/oauth/authorize/?client_id={0}&redirect_uri={1}&response_type=code", client_id, redirect_uri);
            return Redirect(url);
        }

        public ActionResult GetToken(string code)
        {
            var client_id = Session["client_id"].ToString();
            var client_secret = Session["client_secret"].ToString();
            string URI = "https://api.instagram.com/oauth/access_token";
            var param = new NameValueCollection();
            param.Add("client_id", client_id);
            param.Add("client_secret", client_secret);
            param.Add("grant_type", "authorization_code");
            param.Add("redirect_uri", "http://localhost:53938/home/gettoken");
            param.Add("code", code);
            InstagramAccount newInstagram = null;
            if (Session["instagram"] == null)
            {
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        byte[] responsebytes = wc.UploadValues(URI, "POST", param);
                        string responsebody = Encoding.UTF8.GetString(responsebytes);
                        var json = (JObject)JsonConvert.DeserializeObject(responsebody);

                        var accessToken = json["access_token"].ToString();
                        var userName = json["user"]["username"].ToString();
                        var userId = json["user"]["id"].ToString();
                        var full_name = json["user"]["full_name"].ToString();
                        var profile_picture = json["user"]["profile_picture"].ToString();
                        newInstagram = new InstagramAccount
                        {
                            access_token = accessToken,
                            user = new InstagramUser
                            {
                                id = userId,
                                full_name = full_name,
                                profile_picture = profile_picture
                            }
                        };
                        Session["instagram"] = newInstagram;
                        string newInstagramStr = JsonConvert.SerializeObject(newInstagram);
                        if (newInstagram.access_token != "")
                        {
                            System.IO.File.WriteAllText(Server.MapPath("~/instagram.json"), newInstagramStr);
                        }
                        
                        
                    }
                    catch (WebException ex)
                    {
                        using (var responseStream = ex.Response.GetResponseStream())
                        using (var textReader = new StreamReader(responseStream))
                        {
                            var errorMessage = textReader.ReadToEnd();
                        }
                    }
                }
            }
            else
            {
                newInstagram = (InstagramAccount) Session["instagram"];
            }
            

            return View(newInstagram);
        }


        //public ActionResult()
        //{
        //    var authkey = "452455117.f1d83b1.159b681da6df4415b320738356926ee7"
        //    return ;
        //}


         



       


    }
}


public class InstagramAccount
{
    public string access_token { get; set; }
    public InstagramUser user { get; set; }
}

public class InstagramUser
{
    public string id { get; set; }
    public string username { get; set; }
    public string full_name { get; set; }
    public string profile_picture { get; set; }
}