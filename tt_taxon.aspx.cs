using portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Worms;

namespace species
{
    public partial class tt_taxon : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs evr)
        {
            String name = Request["name"];
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
                    Response.Write("[]");
                    Response.End();
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
                Response.Write("[]");
                Response.End();
            }

            for (int i = 0; i < records.Length; i++)
            {
                AphiaRecord record = records[i];
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

            js_list outlist = new js_list();
            foreach (TaxonRecord taxon in list)
            {
                String value = taxon.vernacular + ": " + taxon.scientificname;
                jsresponse item = outlist.add();
                item.add("value", value);
                item.add("text", taxon.vernacular);
            }
            Response.ContentType = "text/json";
            Response.Write(outlist.write());



        }
    }
}