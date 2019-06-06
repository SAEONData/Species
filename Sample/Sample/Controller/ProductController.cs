using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;

namespace Sample.Controller
{
    public class ProductController : ApiController
    {
        [HttpGet]
        [ActionName("GetHelloWorld")]
        public string GetHelloWorld()
        {
            ArrayList al = new ArrayList { "Hello", "World", "From", "Sample", "Application" };
            return JsonConvert.SerializeObject(al);
        }
    }
}