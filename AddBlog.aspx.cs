using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Threading;
using System.Threading.Tasks;
using AVOSCloud;

public partial class AddBlog : System.Web.UI.Page
{
    private readonly string SESSIONKEY = "userlist";
    private IEnumerable<AVUser> users = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!this.Page.IsPostBack)
        {
            AVClient.Initialize("u2usgtzl5t8w9t2qpf8bbc88rvg85g5tgjtja3jpq0gfoilc", "dspdy8ip356aahviafq216hszwb0gp908vov9b4cck7nrywu");
            this.InitParentCategories();
            if (this.Page.Session[SESSIONKEY] == null)
            {
                this.InitUsers();
            }
        }
    }

    async private void InitParentCategories()
    {
        IEnumerable<AVObject> parentCategories = null;
        var task = AVObject.GetQuery("Category").WhereEqualTo("parent", null).FindAsync().ContinueWith(r =>
             {
                 parentCategories = r.Result;
                 
                 var datasource = new DataTable();
                 datasource.Columns.Add("categoryName");
                 datasource.Columns.Add("objectId");

                 foreach (var category in parentCategories)
                 {
                     datasource.Rows.Add(new[] { category["name"], category.ObjectId });
                 }

                this.parent_category.DataSource = datasource;
                this.parent_category.DataTextField = "categoryName";
                this.parent_category.DataValueField = "objectId";
                this.parent_category.DataBind();
             });

        await task;
    }

    async private void InitUsers()
    {
        var task = AVUser.Query.WhereStartsWith("email", "user").FindAsync().ContinueWith(r =>
        {
            users = r.Result;

            var datasource = new DataTable();
            datasource.Columns.Add("username");
            datasource.Columns.Add("objectId");

            foreach (var u in users)
            {
                datasource.Rows.Add(new[] { u["nickname"], u.ObjectId });
            }

            this.choosen_users.DataSource = datasource;
            this.choosen_users.DataTextField = "username";
            this.choosen_users.DataValueField = "objectId";
            this.choosen_users.DataBind();

            this.Page.Session[SESSIONKEY] = users;
        });

        await task;
    }

    async private void InitChildCategories()
    {
        IEnumerable<AVObject> categories = null;
        AVObject parentCategory = null;
        var parentTask = AVObject.GetQuery("Category").GetAsync(this.parent_category.SelectedValue).ContinueWith(r =>
            {
                parentCategory = r.Result;
            });

        await parentTask;

        var task = AVObject.GetQuery("Category").WhereEqualTo("parent", parentCategory).FindAsync().ContinueWith(r =>
        {
            categories = r.Result;

            var datasource = new DataTable();
            datasource.Columns.Add("categoryName");
            datasource.Columns.Add("objectId");

            foreach (var category in categories)
            {
                datasource.Rows.Add(new[] { category["name"], category.ObjectId });
            }

            this.child_category.DataSource = datasource;
            this.child_category.DataTextField = "categoryName";
            this.child_category.DataValueField = "objectId";
            this.child_category.DataBind();
        });

        await task;
    }

    protected void parent_category_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.InitChildCategories();
    }

    private Rectangle fullSizeRect = new Rectangle(0, 0, 400, 400);
    private Rectangle thumbnailSizeRect = new Rectangle(0, 0, 91, 91);   

    private System.Drawing.Image ResizeImageWithSize(Stream originalImageStream, Rectangle sizeRect)
    {
        System.Drawing.Image image = System.Drawing.Image.FromStream(originalImageStream);
        System.Drawing.Image newImage = null;
        Graphics graphics = null;
        try
        {
            newImage = new Bitmap(sizeRect.Width, sizeRect.Height);
            graphics = Graphics.FromImage(newImage);

            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;

            graphics.DrawImage(image, sizeRect, new RectangleF(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
        }
        finally
        {
            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }
        }

        return newImage;
    }

    private byte[] GetBytesFromImage(System.Drawing.Image image)
    {
        MemoryStream ms = new MemoryStream();
        byte[] data = null;
        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
        data = ms.GetBuffer();
        return data;
    }

    async private void UploadPictures()
    {
        List<AVUser> selectedUserList = new List<AVUser>();

        if (this.Page.Session[SESSIONKEY] == null)
        {
            this.InitUsers();
        }

        var users = (IEnumerable<AVUser>)this.Page.Session[SESSIONKEY];
        foreach (ListItem item in this.choosen_users.Items)
        {
            if (item.Selected)
            {
                AVUser user = users.First(u => u.ObjectId == item.Value);
                selectedUserList.Add(user);
            }
        }

        Random random = new Random();
        int userCount = selectedUserList.Count;

        AVObject selectedCategory = await AVObject.GetQuery("Category").GetAsync(this.child_category.SelectedValue);

        var uploadedFiles = this.file_upload.PostedFiles;
        foreach (var file in uploadedFiles)
        {
            var fullSizeImage = this.ResizeImageWithSize(file.InputStream, fullSizeRect);
            var thumbnailImage = this.ResizeImageWithSize(file.InputStream, thumbnailSizeRect);

            AVFile imageFull = new AVFile(file.FileName, this.GetBytesFromImage(fullSizeImage));
            await imageFull.SaveAsync();

            AVFile imageThumbnail = new AVFile(string.Format("thumbnail_{0}", file.FileName), this.GetBytesFromImage(thumbnailImage));
            await imageThumbnail.SaveAsync();

            this.SaveBlog(selectedUserList[random.Next(userCount)], selectedCategory.ObjectId, imageThumbnail.ObjectId, imageFull.ObjectId);
        }
    }

    private string jsonString = "[\"username\":\"{0}\", \"category\": [ \"__type\": \"Pointer\", \"className\": \"Category\", \"objectId\": \"{1}\"], \"user\": [ \"__type\": \"Pointer\", \"className\": \"_User\", \"objectId\": \"{2}\" ], \"thumbnail\": [ \"__type\": \"File\", \"id\": \"{3}\"], \"image\": [ \"__type\": \"File\", \"id\": \"{4}\" ]]";

    private void SaveBlog(AVObject user, string categoryId, string thumbnailId, string imageId)
    {
        string uri = "https://api.leancloud.cn/1.1/classes/Blog";
        string requestString = string.Format(jsonString, user["nickname"], categoryId, user.ObjectId, thumbnailId, imageId);
        requestString = requestString.Replace('[', '{').Replace(']', '}');

        WebClient client = new WebClient();
        client.Headers.Add("Content-Type", "application/json");
        client.Headers.Add("X-AVOSCloud-Application-Id", "u2usgtzl5t8w9t2qpf8bbc88rvg85g5tgjtja3jpq0gfoilc");
        client.Headers.Add("X-AVOSCloud-Application-Key", "dspdy8ip356aahviafq216hszwb0gp908vov9b4cck7nrywu");

        var responseStr = client.UploadString(uri, requestString);
        this.error_label.Text = responseStr;
        this.error_label.Visible = true;
    }

    protected void btn_upload_Click(object sender, EventArgs e)
    {
        this.UploadPictures();
    }
}