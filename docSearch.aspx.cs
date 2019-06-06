using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;
using System.Collections;

namespace species
{
	//Class for binding the search results to the repeater
		
    public class DocSearch : System.Web.UI.Page
    {
		protected System.Web.UI.WebControls.TextBox txtSearch;
		protected System.Web.UI.WebControls.LinkButton cmdPrev, cmdNext;
		protected System.Web.UI.WebControls.Repeater searchResults;
		protected System.Web.UI.WebControls.Label lbl, resultSummary;
		protected ArrayList values = new ArrayList();
		public int cnt;
		
		public int CurrentPage
		{
			get	{
				
				object o = this.ViewState["_CurrentPage"];  // look for current page in ViewState
				if (o == null)
				return 0;   // default to showing the first page
				else
				return (int) o;
			}
			
			set {
				this.ViewState["_CurrentPage"] = value;
			}
		}
		
		//Execute the search
		public void btnSearch_Click(object sender, CommandEventArgs e)
		{
			//Reset to page 1 as a new search is performed
			if(e.CommandName == "search")
				CurrentPage = 0;
			
			// Show Prev or Next buttons upon searching
			cmdPrev.Visible = true;
			cmdNext.Visible = true;
			
			cnt = 1;
			if(txtSearch.Text.Trim() == "")
				btnClear_Click(sender, e);
			else
				Search();
		}
		
		//Clear the search text and refresh the page to clear the search results
		public void btnClear_Click(object sender, CommandEventArgs e)
		{
			txtSearch.Text = "";
			Response.Redirect(Request.ServerVariables["URL"]);
		}
		
		public void Search()
		{
			//create a connection object and command object, to connect the Index Server
			System.Data.OleDb.OleDbConnection odbSearch =  new System.Data.OleDb.OleDbConnection( "Provider=\"MSIDXS\";Data Source=\"docSearch\";");
			System.Data.OleDb.OleDbCommand cmdSearch =  new System.Data.OleDb.OleDbCommand();
			//assign connection to command object cmdSearch 
			cmdSearch.Connection = odbSearch;
			
			//Query to search a free text string in the catalog in the contents of the indexed documents in the catalog 
			string searchText = txtSearch.Text.Replace("'","''");
			cmdSearch.CommandText = "select doctitle, filename, vpath, rank, characterization from scope() where FREETEXT(Contents, '"+ searchText +"') order by rank desc ";

			odbSearch.Open();
			
			try{
				//execute search query
				OleDbDataReader rdrSearch = cmdSearch.ExecuteReader();
				//loop through each result and bind it to the repeater control
				while( rdrSearch.Read())
				{
					//Assemble the search result text and abstract
					getpagelink(rdrSearch[0].ToString(), rdrSearch[2].ToString(), rdrSearch[4].ToString());
				}
			}
			catch(Exception ex) 
			{
				lbl.Text= "Search Error: " + ex.Message + "<br>" ;
			}
			
			odbSearch.Close();
			
			// Populate the repeater control with the Items DataSet
			PagedDataSource objPds = new PagedDataSource();
			objPds.DataSource = values;
			
			// Indicate that the data should be paged
			objPds.AllowPaging = true;
			
			// Set the number of items you wish to display per page
			objPds.PageSize = 10; 
			
			// Set the PagedDataSource's current page
			objPds.CurrentPageIndex = CurrentPage;
			
			//Display a summary for the current search
			resultSummary.Text = "Your search for <b>" + txtSearch.Text + "</b> Returned " + (cnt-1) + " Results.  " + 
				"<br>Page: " + (CurrentPage + 1).ToString() + " of " + objPds.PageCount.ToString();
				
			// Disable Prev or Next buttons if necessary
			cmdPrev.Enabled = !objPds.IsFirstPage;
			cmdNext.Enabled = !objPds.IsLastPage;
	 
			searchResults.DataSource = objPds;
			searchResults.DataBind();
		}

		//Assemble the elements of the search result into a formatted string
		public void getpagelink(string srcfile, string url, string fileAbstract) 
		{
			string temp= cnt.ToString() +".) " + "<a href=\"" + url + "\" target=\"_blank\">" + srcfile + "</a>";	
			values.Add(new SearchResult(temp, fileAbstract)); 
			cnt += 1; 
		}
		
		//Go to previous result page
		public void cmdPrev_Click(object sender, CommandEventArgs e)
		{
			// Set viewstate variable to the previous page
			CurrentPage -= 1;
			// Reload control
			btnSearch_Click(sender, e);
		}
		
		//Go to next result page
		public void cmdNext_Click(object sender, CommandEventArgs e)
		{
			// Set viewstate variable to the next page
			CurrentPage += 1;
			// Reload control
			btnSearch_Click(sender, e);
		}
	}
}