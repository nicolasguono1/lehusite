using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
                datasource.Rows.Add(new[] { blog["username"], blog["report_count"], ((AVFile)blog["thumbnail"]).Url, blog.ObjectId });
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

    private string jsonString = "[\"accepted\":true, \"isread\":true, \"userfrom\": [ \"__type\": \"Pointer\", \"className\": \"_User\", \"objectId\": \"{0}\"], \"userto\": [ \"__type\": \"Pointer\", \"className\": \"_User\", \"objectId\": \"{1}\" ]]";
    private List<string> OurUserList = new List<string> { "54d6f366e4b029bade939875", "551e5404e4b0cd5b6244f29b", "54c8b2f8e4b029264cb66142", "54edb001e4b0b005716d7f7b", "54c9c8d4e4b0c6c6afb5f323" };

    async private void CreateUser()
    {
        string uri = "https://api.leancloud.cn/1.1/classes/Friend";
        string requestString = "";

        for (int i = 0; i < 60; i++)
        {
            var user = new AVUser();
            user.Username = string.Format("TestUser{0}", i);
            user.Password = "123456";
            user.Email = string.Format("user{0}@126.com", i);
            await user.SignUpAsync();

            foreach (var u in OurUserList)
            {
                WebClient client = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                client.Headers.Add("X-AVOSCloud-Application-Id", "u2usgtzl5t8w9t2qpf8bbc88rvg85g5tgjtja3jpq0gfoilc");
                client.Headers.Add("X-AVOSCloud-Application-Key", "dspdy8ip356aahviafq216hszwb0gp908vov9b4cck7nrywu");

                requestString = string.Format(jsonString, u, user.ObjectId);
                requestString = requestString.Replace('[', '{').Replace(']', '}');
                client.UploadString(uri, requestString);               
            }
        }
    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        CreateUser();
    }
}