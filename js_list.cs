using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace portal
{
    public class js_list
    {
        public List<jsresponse> array = new List<jsresponse>();
        public jsresponse add()
        {
            jsresponse obj = new jsresponse();
            array.Add(obj);
            return obj;
        }

        public String write()
        {
            string code = "";
            for (int i=0; i<array.Count; i++) 
            {
                if (code != "")
                    code += ", ";
                code += array[i].write();
            }
            return "[" + code + "]";
        }



    }
}