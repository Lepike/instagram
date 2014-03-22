using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Instagram.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Instagram.Controllers
{
    public class InstagramController : Controller
    {
        //
        // GET: /Instagram/

        public ActionResult Index()
        {
            return View();
        }

        private string LoadJson(string url)
        {
            string json;
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                json = client.DownloadString(url);
            }
            return json;
        }

        public JsonResult Recent()
        {
            var instagramImages = new List<InstagramImage>();
            string filePath = Request.MapPath("~/instagram.txt");
            var test = System.IO.File.Exists(filePath);
            if (System.IO.File.Exists(filePath))
            {
                using (StreamReader r = new StreamReader(filePath))
                {
                    string AccountJson = r.ReadToEnd();
                    var instagramObj = JsonConvert.DeserializeObject<InstagramAccount>(AccountJson);
                    var url = string.Format("https://api.instagram.com/v1/users/{0}/media/recent/?access_token={1}", instagramObj.user.id, instagramObj.access_token);

                    using (WebClient wc = new WebClient())
                    {
                        try
                        {
                            //var param = new NameValueCollection();
                            //param.Add("CLIENT_ID", instagramObj.access_token);
                            //param.Add("COUNT", "3");
                            //byte[] responsebytes = wc.UploadValues(url, "GET", param);
                            //string responsebody = Encoding.UTF8.GetString(responsebytes);
                            var responsebody = LoadJson(url);
                            var json = (JObject)JsonConvert.DeserializeObject(responsebody);
                            var responseObj = JsonConvert.DeserializeObject<dynamic>(responsebody);
                            
                            var arrayOfImages = (JArray)responseObj.data;
                            instagramImages = arrayOfImages.ToObject<List<InstagramImage>>();


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
            }

            return Json(instagramImages, JsonRequestBehavior.AllowGet);
        }

    }
}
