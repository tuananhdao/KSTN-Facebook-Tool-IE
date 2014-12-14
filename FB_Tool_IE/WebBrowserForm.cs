using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KSTN_Facebook_Tool
{
    public partial class WebBrowserForm : Form
    {
        public string access_token;
        private String user;
        private String pass;
        public String user_id;
        public DataTable dt;
        public bool busy = false;

        Dictionary<String, String> links = new Dictionary<string, string>();

        public WebBrowserForm()
        {
            InitializeComponent();
            dt = new DataTable();
            Program.mainForm.dgGroups.DataSource = null;
            dt.Columns.Add("group_name");
            dt.Columns.Add("group_link");
            dt.Columns.Add("group_mem");

            // URLs
            links["fb_url"] = "https://mbasic.facebook.com";
            links["fb_get_token"] = "https://www.facebook.com/dialog/oauth?client_id=145634995501895&redirect_uri=https%3A%2F%2Fdevelopers.facebook.com%2Ftools%2Fexplorer%2Fcallback&response_type=token&scope=publish_actions,publish_stream,user_groups,user_friends";
            links["fb_groups"] = links["fb_url"] + "/browsegroups/?seemore";
            links["facebook_graph"] = "http://graph.facebook.com";
        }

        public void Exceptions_Handler()
        {
            try
            {
                Program.loadingForm.Hide();
                MessageBox.Show("Kiểm tra lại thông tin đăng nhập và đường truyền mạng của bạn! Nhấn OK để đóng ứng dụng...");
                Process.GetCurrentProcess().Kill();
            }
            catch
            { }
        }

        public void Fb_Login(String _user, String _pass)
        {
            Program.loadingForm.setText("Đang đăng nhập tài khoản Facebook...");
            Program.loadingForm.Show();
            clear_cookies();
            user = _user;
            pass = _pass;
            wb.Navigate(links["fb_url"]);
            Program.mainForm.btnLogin.Enabled = false;
            Program.mainForm.btnLogin.Text = "Đang đăng nhập...";
            wb.DocumentCompleted += Fb_Login_Completed;
        }

        private void Fb_Login_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                HtmlElement textElement = wb.Document.All.GetElementsByName("email")[0];
                textElement.SetAttribute("value", user);
                HtmlElement pwdElement = wb.Document.All.GetElementsByName("pass")[0];
                pwdElement.SetAttribute("value", pass);
                HtmlElement btnLoginElement = wb.Document.All.GetElementsByName("login")[0];
                btnLoginElement.InvokeMember("click");
            }
            catch
            {
                Exceptions_Handler();
            }
            wb.DocumentCompleted -= Fb_Login_Completed;
            wb.DocumentCompleted += this.Fb_Feed_Completed;
        }

        private void Fb_Feed_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                if (wb.Url.ToString().Contains("home.php") || wb.Url.ToString().Contains("phoneacquire"))
                {
                    Program.mainForm.btnLogin.Text = "Đăng nhập thành công!";
                    Program.mainForm.txtUser.Enabled = false;
                    Program.mainForm.txtPass.Enabled = false;
                    Program.mainForm.btnPost.Enabled = true;
                    Program.mainForm.btnInvite.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Kiểm tra lại thông tin đăng nhập!");
                    Exceptions_Handler();
                }
            }
            catch
            {
                Exceptions_Handler();
            }
            wb.DocumentCompleted -= Fb_Feed_Completed;
            get_FB_groups();
        }

        public void get_FB_access_token()
        {
            wb.Navigate(links["fb_get_token"]);
            wb.DocumentCompleted += get_FB_access_token_Completed;
        }

        private void get_FB_access_token_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                HtmlElement btnLoginElement = Program.wbf.wb.Document.All.GetElementsByName("__CONFIRM__")[0];
                btnLoginElement.InvokeMember("click");
            }
            catch
            {
                Exceptions_Handler();
            }
            wb.DocumentCompleted -= get_FB_access_token_Completed;
        }

        private void wb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            /*
            HtmlElement textElement = wb.Document.All.GetElementsByName("q")[0];
            textElement.SetAttribute("value", "your text to search");
            HtmlElement btnElement = wb.Document.All.GetElementsByName("btnG")[0];
            btnElement.InvokeMember("click");

            HtmlElementCollection classButton = wb.Document.All;
            foreach (HtmlElement element in classButton)
            {
                if (element.GetAttribute("className") == "button")
                {
                    element.InvokeMember("click");
                }
            }

            wb.Document.GetElementById("gs_tti0").InnerText = "hello world";
            */

            wb.DocumentCompleted -= wb_DocumentCompleted;
        }

        private void WebBrowserForm_Shown(object sender, EventArgs e)
        {
            clear_cookies();
        }

        private void clear_cookies()
        {
            // Clear Cookies
            string[] Cookies = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            int notDeleted = 0;
            foreach (string CookieFile in Cookies)
            {
                try
                {
                    System.IO.File.Delete(CookieFile);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi Clear Cookie!" + ex);
                    notDeleted++;
                }

            }
        }

        public void get_FB_groups()
        {
            wb.Navigate(links["fb_groups"]);
            wb.DocumentCompleted += get_FB_groups_Completed;
        }

        private void get_FB_groups_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= get_FB_groups_Completed;
            loadGroups();
        }

        private async Task getUserAndGroups()
        {
            try
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(wb.DocumentStream);
                var nodes = doc.DocumentNode.SelectNodes("//table//tbody//tr//td//div")[3].SelectNodes(".//li//table//tbody//tr//td[@class='o']//a");
                if (nodes.Count != 0)
                {
                    foreach (HtmlAgilityPack.HtmlNode node in nodes)
                    {
                        //dt.Rows.Add(node.ChildNodes[0].InnerHtml, node.GetAttributeValue("href", ""), "");
                        await Task.Factory.StartNew(() => addTableRow(node));
                    }
                }
                else
                {
                    Exceptions_Handler();
                }
                Program.mainForm.dgGroups.DataSource = dt;
                Program.mainForm.dgGroupInvites.DataSource = dt;

                nodes = doc.DocumentNode.SelectNodes("//div[@id='header']//div//a");
                Match match = Regex.Match(nodes[2].GetAttributeValue("href", ""), @"/([A-Za-z0-9\-]+)\?ref_component", RegexOptions.None);
                if (match.Success)
                {
                    user_id = match.Groups[1].Value;
                    Program.mainForm.pbAvatar.Load(links["facebook_graph"] + "/" + user_id + "/picture");
                    Program.mainForm.lblViewProfile.Text = "https://facebook.com/" + user_id;
                }

                nodes = doc.DocumentNode.SelectNodes("//td[@style]//a");
                if (nodes.Count == 5)
                {
                    match = Regex.Match(nodes[4].InnerHtml, @"\((.*)\)$", RegexOptions.None);
                    if (match.Success)
                    {
                        Program.mainForm.lblUsername.Text = match.Groups[1].Value;
                    }
                }
            }
            catch
            {
                get_FB_groups();
                Console.WriteLine("Get FB Groups: Try Again...");
                // try until success
            }
            Program.mainForm.lblProgress.Text = "0/" + dt.Rows.Count;
            await TaskEx.Delay(2000);
        }

        private void addTableRow(HtmlAgilityPack.HtmlNode node)
        {
            dt.Rows.Add(node.ChildNodes[0].InnerHtml, node.GetAttributeValue("href", ""), "");
        }

        private async Task loadGroups()
        {
            Program.loadingForm.setText("Tải thông tin cá nhân và danh sách nhóm...");
            await getUserAndGroups();
            Program.loadingForm.Hide();
        }

        public async Task AutoPost()
        {
            if (Program.mainForm.btnInvite.Enabled == false)
            {
                MessageBox.Show("Chỉ có thể chạy cùng lúc 1 chức năng!");
                return;
            }

            if (busy == true)
                return;

            int delay;
            int progress = 0;

            if (!int.TryParse(Program.mainForm.txtDelay.Text, out delay) || delay < 30)
            {
                MessageBox.Show("Số giây Delay: số nguyên không nhỏ hơn 30");
                Exceptions_Handler();
            }

            foreach (DataRow row in dt.Rows)
            {
                while (busy == true)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Program.mainForm.lblTick.Text = 10 - i + " (!Mạng chậm)";
                        await TaskEx.Delay(1000);
                    }
                }

                progress++;
                Program.mainForm.lblProgress.Text = progress + "/" + dt.Rows.Count;

                busy = true;
                wb.Navigate(links["fb_url"] + row.ItemArray[1]);
                wb.DocumentCompleted += AutoPost_Completed;

                Program.mainForm.lblPostingGroup.Text = row.ItemArray[0].ToString();

                for (int i = 0; i < delay; i++)
                {
                    Program.mainForm.lblTick.Text = delay - i + "";
                    await TaskEx.Delay(1000);
                }
            }
        }

        private void AutoPost_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= AutoPost_Completed;
            if (Program.mainForm.txtBrowse1.Text == "" && Program.mainForm.txtBrowse2.Text == "" && Program.mainForm.txtBrowse3.Text == "")
            {
                // Không ảnh
                if (Program.mainForm.txtContent.Text != "")
                {
                    HtmlElement message = wb.Document.All.GetElementsByName("xc_message")[0];
                    message.SetAttribute("value", Program.mainForm.txtContent.Text);
                    HtmlElement btnViewPost = wb.Document.All.GetElementsByName("view_post")[0];
                    btnViewPost.InvokeMember("click");
                    wb.DocumentCompleted += AutoPost_Result_Completed;
                }
                else
                {
                    MessageBox.Show("Điền nội dung trước khi post bài!");
                    Exceptions_Handler();
                }
            }
            else
            {
                // Có ảnh
                try
                {
                    HtmlElement btnViewPhoto = wb.Document.All.GetElementsByName("lgc_view_photo")[0];
                    btnViewPhoto.InvokeMember("click");
                    wb.DocumentCompleted += AutoPost_Photo_Completed;
                }
                catch
                {
                    busy = false;
                    return;
                }
            }
        }

        private void AutoPost_Photo_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= AutoPost_Photo_Completed;
            Populate();
        }

        private void AutoPost_Result_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            String result_url = "";
            Match match = Regex.Match(wb.Url + "", @"\?photo_fbid\=([A-Za-z0-9\-]+)\&id\=", RegexOptions.None);
            if (match.Success)
            {
                result_url = match.Groups[1].Value;
            }
            Program.mainForm.dgPostResult.Rows.Add(Program.mainForm.lblPostingGroup.Text, result_url);
            wb.DocumentCompleted -= AutoPost_Result_Completed;
            busy = false;
        }

        async Task PopulateInputFile(HtmlElement file, String photo)
        {
            file.Focus();

            // delay the execution of SendKey to let the Choose File dialog show up
            var sendKeyTask = TaskEx.Delay(500).ContinueWith((_) =>
            {
                // this gets executed when the dialog is visible
                SendKeys.SendWait(photo + "{ENTER}");
            }, TaskScheduler.FromCurrentSynchronizationContext());

            file.InvokeMember("Click"); // this shows up the dialog

            await sendKeyTask;
            file.RemoveFocus();

            // delay continuation to let the Choose File dialog hide
            await TaskEx.Delay(500);
        }

        async Task Populate()
        {
            try
            {
                HtmlElement file1 = wb.Document.All.GetElementsByName("file1")[0];
                HtmlElement file2 = wb.Document.All.GetElementsByName("file2")[0];
                HtmlElement file3 = wb.Document.All.GetElementsByName("file3")[0];
                if (Program.mainForm.txtBrowse1.Text != "")
                    await PopulateInputFile(file1, Program.mainForm.txtBrowse1.Text);
                if (Program.mainForm.txtBrowse2.Text != "")
                    await PopulateInputFile(file2, Program.mainForm.txtBrowse2.Text);
                if (Program.mainForm.txtBrowse3.Text != "")
                    await PopulateInputFile(file3, Program.mainForm.txtBrowse3.Text);

                HtmlElement message = wb.Document.All.GetElementsByName("xc_message")[0];
                message.SetAttribute("value", Program.mainForm.txtContent.Text);

                HtmlElement btnPhotoUpload = wb.Document.All.GetElementsByName("photo_upload")[0];
                btnPhotoUpload.InvokeMember("click");
            }
            catch
            {
                busy = false;
                return;
            }
            wb.DocumentCompleted += AutoPost_Result_Completed;
        }

        public async Task AutoInvite()
        {
            if (Program.mainForm.btnPost.Enabled == false)
            {
                MessageBox.Show("Chỉ có thể chạy cùng lúc 1 chức năng!");
                return;
            }
            if (Program.mainForm.txtInviteName.Text == "")
            {
                MessageBox.Show("Nhập tên người muốn mời!");
                return;
            }
            int delay;
            if (!int.TryParse(Program.mainForm.txtInviteDelay.Text, out delay) || delay < 30)
            {
                MessageBox.Show("Nhập số nguyên Delay không nhỏ hơn 30");
                return;
            }

            Program.mainForm.btnInvite.Enabled = false;
            Program.mainForm.txtInviteName.Enabled = false;
            Program.mainForm.txtInviteDelay.Enabled = false;

            int progress = 0;

            foreach (DataRow row in dt.Rows)
            {
                while (busy == true)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        await TaskEx.Delay(1000);
                        Program.mainForm.lblTick2.Text = 10 - i + " (!)";
                    }
                }
                progress++;
                Program.mainForm.lblProgress2.Text = progress + "/" + dt.Rows.Count;
                Program.mainForm.lblInviting.Text = row["group_name"] + "";

                busy = true;

                //await Task.Factory.StartNew(() => AutoInviteTask(row["group_link"] + ""));
                AutoInviteTask(row["group_link"] + "");
                for (int i = 0; i < delay; i++)
                {
                    Program.mainForm.lblTick2.Text = delay - i + "";
                    await TaskEx.Delay(1000);
                }
            }
        }

        private void AutoInviteTask(String group_url)
        {
            String group_id = group_url.Substring(8);
            wb.Navigate("https://mbasic.facebook.com/groups/members/search/?group_id=" + group_id + "&refid=18");
            wb.DocumentCompleted += AutoInvite_Completed;
        }

        private void AutoInvite_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= AutoInvite_Completed;
            try
            {
                HtmlElement name = wb.Document.All.GetElementsByName("query_term")[0];
                name.SetAttribute("value", Program.mainForm.txtInviteName.Text);
                HtmlElement form = wb.Document.GetElementsByTagName("form")[1];
                HtmlElement btnSearch = form.GetElementsByTagName("input")[4];
                btnSearch.InvokeMember("click");
                wb.DocumentCompleted += AutoInvite2_Completed;
            }
            catch
            {
                busy = false;
                return;
            }

        }

        private void AutoInvite2_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= AutoInvite2_Completed;

            try
            {
                HtmlElement form = wb.Document.GetElementsByTagName("form")[2];
                HtmlElement div = form.GetElementsByTagName("div")[0];
                HtmlElement input = div.GetElementsByTagName("input")[0];
                input.InvokeMember("click");
                HtmlElementCollection btnSubmits = form.GetElementsByTagName("input");
                HtmlElement btnSubmit = btnSubmits[btnSubmits.Count - 1];
                btnSubmit.InvokeMember("click");
                wb.DocumentCompleted += AutoInvite3_Completed;
            }
            catch
            {
                busy = false;
                return;
            }
        }

        private void AutoInvite3_Completed(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wb.DocumentCompleted -= AutoInvite3_Completed;
            Program.mainForm.dgInvitedGroups.Rows.Add(Program.mainForm.lblInviting.Text);
            busy = false;
        }
    }
}
