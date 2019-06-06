using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace portal
{
    public class jsresponse
    {
        Dictionary<string, object> vals = new Dictionary<string, object>();

        public void add(String name, object val)
        {
            vals[name] = val;
        }

        public String write()
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(vals);
        }
    }
}