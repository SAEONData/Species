using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;

namespace species
{
    /// <summary>
    /// Summary description for worms
    /// </summary>
    public class worms : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            String name = context.Request["name"];
            List<TaxonRecord> list = new List<TaxonRecord>();

            // get records from worms service
            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();

            Worms.AphiaRecord[] records = null;

            // check if the AphiaID is given
            if (name.IndexOf(':') != -1 && name.IndexOf('(') != -1 && name.IndexOf(')') != -1)
            {
                int s = name.IndexOf('(');
                int e = name.IndexOf(')');
                int AphiaID = int.Parse(name.Substring(s + 1, e - s - 1));
                Worms.AphiaRecord record = client.getAphiaRecordByID(AphiaID);
                if (record == null)
                {
                    context.Response.Write("[]");
                    context.Response.End();
                }
                List<Worms.AphiaRecord> alist = new List<Worms.AphiaRecord>();
                alist.Add(record);
                records = alist.ToArray();
            }
            else
            {
                records = client.getAphiaRecordsByVernacular(name, true, 0);
            }

            if (records == null)
            {
                context.Response.Write("[]");
                context.Response.End();
            }

            for (int i = 0; i < records.Length; i++)
            {
                Worms.AphiaRecord record = records[i];
                TaxonRecord taxon = new TaxonRecord();
                taxon.AphiaID = record.AphiaID;
                taxon.authority = record.authority;
                taxon.citation = record.citation;
                taxon.@class = record.@class;
                taxon.family = record.family;
                taxon.genus = record.genus;
                taxon.isBrackish = record.isBrackish;
                taxon.isExtinct = record.isExtinct;
                taxon.isFreshwater = record.isFreshwater;
                taxon.kingdom = record.kingdom;
                taxon.lsid = record.lsid;
                taxon.match_type = record.match_type;
                taxon.modified = record.modified;
                taxon.order = record.order;
                taxon.phylum = record.phylum;
                taxon.rank = record.rank;
                taxon.scientificname = record.scientificname;
                taxon.status = record.status;
                taxon.url = record.url;
                taxon.valid_name = record.valid_name;
                taxon.vernacular = "???";
                list.Add(taxon);
            }

            // spawn threads to get vernacular name
            foreach (TaxonRecord taxon in list)
            {
                taxon.thread = new Thread(taxon.GetVernacular);
                taxon.thread.Start();
            }

            // wait to all treads to complete
            foreach (TaxonRecord taxon in list)
            {
                taxon.thread.Join();
                taxon.thread = null;
            }

            var json = new JavaScriptSerializer().Serialize(list);

            context.Response.ContentType = "text/json";
            context.Response.Write(json);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class TaxonClass
    {
        public string rank;
        public string name;
    };



    public class TaxonRecord : Worms.AphiaRecord
    {
        public string vernacular;
        public Thread thread;
        public List<TaxonClass> taxon;


        public void GetVernacular()
        {
            Worms.AphiaNameServicePortTypeClient client = new Worms.AphiaNameServicePortTypeClient();

            // get the common name
            Worms.Vernacular[] vec = client.getAphiaVernacularsByID(AphiaID);
            if (vec != null && vec.Length > 0)
            {
                this.vernacular = vec[0].vernacular;
            }

            // get classification
            Worms.Classification wclass = client.getAphiaClassificationByID(AphiaID);
            if (wclass != null)
            {
                taxon = new List<TaxonClass>();
                while (wclass != null)
                {
                    TaxonClass tc = new TaxonClass();
                    tc.rank = wclass.rank;
                    tc.name = wclass.scientificname;
                    taxon.Add(tc);
                    wclass = wclass.child;
                }
            }

            
        }
    }

}