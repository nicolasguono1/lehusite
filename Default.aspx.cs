using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AVOSCloud;

public partial class _Default : System.Web.UI.Page
{
    private DataTable datasource = null;
    private string report_count_key = "report_count";
    protected void Page_Load(object sender, EventArgs e)
    {
        AVClient.Initialize("u2usgtzl5t8w9t2qpf8bbc88rvg85g5tgjtja3jpq0gfoilc", "dspdy8ip356aahviafq216hszwb0gp908vov9b4cck7nrywu");
        this.InitPictures();
    }

    async private void InitPictures()
    {
        IEnumerable<AVObject> blogs = null;
        var task = AVObject.GetQuery("Blog").WhereGreaterThan(this.report_count_key, 0).OrderByDescending(this.report_count_key).FindAsync().ContinueWith(r => 
        {
            blogs = r.Result;
            datasource = new DataTable();
            datasource.Columns.Add("用户名");
            datasource.Columns.Add("举报数");
            datasource.Columns.Add("图片");
            datasource.Columns.Add("objectId");

            foreach (var blog in blogs)
            {
                datasource.Rows.Add(new[] { blog["username"], blog["report_count"], ((AVFile)blog["image"]).Url, blog.ObjectId });
            }

            this.gridview_picture.DataSource = datasource;
            this.gridview_picture.DataBind();
            this.gridview_user.Visible = false;
            this.gridview_picture.Visible = true;
        });

        await task;
    }

    async private void InitUsers()
    {
        IEnumerable<AVUser> users = null;
        var task = AVUser.Query.WhereGreaterThan(this.report_count_key, 0).OrderByDescending(this.report_count_key).FindAsync().ContinueWith(r =>
        {
            users = r.Result;
            datasource = new DataTable();
            datasource.Columns.Add("用户名");
            datasource.Columns.Add("被举报数");
            datasource.Columns.Add("userId");
            
            foreach (var user in users)
            {
                datasource.Rows.Add(new[] { user["nickname"], user["report_count"], user.ObjectId });
            }

            this.gridview_user.DataSource = datasource;
            this.gridview_user.DataBind();
            this.gridview_picture.Visible = false;
            this.gridview_user.Visible = true;
        });

        await task;
    }

    protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddl_type.SelectedIndex == 0)
        {
            this.InitPictures();
        }
        else
        {
            this.InitUsers();
        }
    }

    protected void LinkButton1_Click(object sender, EventArgs e)
    {
        LinkButton button = (LinkButton)sender;
        this.DeletePicture(button.CommandArgument);
        //this.gridview_picture.r
    }

    async private void DeletePicture(string objectId)
    {
        AVObject blog = null;
        await AVObject.GetQuery("Blog").GetAsync(objectId).ContinueWith(r =>
            {
                blog = r.Result;
                blog.DeleteAsync();
            });

    }

    async private void DeleteUser(string objectId)
    {
        var dict = new Dictionary<string, object>();
        dict["userId"] = objectId;

        var callTask = AVCloud.CallFunctionAsync<string>("deleteUser", dict);
        await callTask;
    }

    protected void btn_deleteuser_Click(object sender, EventArgs e)
    {
        LinkButton button = (LinkButton)sender;
        this.DeleteUser(button.CommandArgument);
    }
}