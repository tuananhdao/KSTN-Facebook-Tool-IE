using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace KSTN_Facebook_Tool
{
    /**
     * 
     * This is a helper class to build postData header an body with the values from 
     * the given HTML form. The purpose of this class is to attach files to 
     * the <INPUT TYPE="FILE"> fields.
     * 
     * License: Code Project Open License (CPOL)
     * (c) Kirill Hryapin, 2008
     * (c) Steven Cheng (Microsoft), 2005 (?)
     * 
     * See usage example in Form1.cs included with this distribution.
     * 
     */

    public class FormToMultipartPostData
    {
        private struct ValuePair // KeyValuePair<string, string> sounds too verbose for me
        {
            public string name;
            public string value;
            public ValuePair(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
        }

        public struct RequestParameters
        {
            public byte[] data;
            public string headers;
        }

        private List<ValuePair> values = new List<ValuePair>();
        private List<ValuePair> files = new List<ValuePair>();
        private Dictionary<string, string> overloadedFiles = new Dictionary<string,string>();

        private HtmlElement form;
        private WebBrowser webbrowser;

        /**
         * In most circumstances, this constructor is better (allows to use Submit() method)
         */
        public FormToMultipartPostData(WebBrowser b, HtmlElement f)
        {
            form = f; webbrowser = b;
            GetValuesFromForm(f);
        }

        /**
         * Use this constructor if you don't want to use Submit() method
         */
        public FormToMultipartPostData(HtmlElement f)
        {
            GetValuesFromForm(f);
        }

        /**
         * Submit the form
         */
        public void Submit()
        {
            Uri url = new Uri(webbrowser.Url, form.GetAttribute("action"));
            RequestParameters req = GetEncodedPostData();
            webbrowser.Navigate(url, form.GetAttribute("target"), req.data, req.headers);
        }

        /**
         * Load values from form
         */
        private void GetValuesFromForm(HtmlElement form)
        {
            // Get values from the form
            foreach (HtmlElement child in form.All)
            {
                switch (child.TagName)
                {
                    case "INPUT":
                        switch (child.GetAttribute("type").ToUpper())
                        {
                            case "FILE":
                                AddFile(child.Name, child.GetAttribute("value"));
                                break;
                            case "CHECKBOX":
                            case "RADIO":
                                if (child.GetAttribute("checked") == "True")
                                {
                                    AddValue(child.Name, child.GetAttribute("value"));
                                }
                                break;
                            case "BUTTON":
                            case "IMAGE":
                            case "RESET":
                                break; // Ignore those?
                            default:
                                AddValue(child.Name, child.GetAttribute("value"));
                                break;
                        }
                        break;
                    case "TEXTAREA":
                    case "SELECT": // it's legal in IE to use .value with select (at least in IE versions 3 to 7)
                        AddValue(child.Name, child.GetAttribute("value"));
                        break;
                } // of "switch tagName"
            } // of "foreach form child"
        }

        private void AddValue(string name, string value)
        {
            if (name == "") return; // e.g. unnamed buttons
            values.Add(new ValuePair(name, value));
        }

        private void AddFile(string name, string value)
        {
            if (name == "") return;
            files.Add(new ValuePair(name, value));
        }

        /**
         * Set file field value [the reason why this class exist]
         */
        public void SetFile(string fieldName, string filePath)
        {
            this.overloadedFiles.Add(fieldName, filePath);
        }

        /**
         * One may need it to know whether there's specific file input
         * For example, to perform some actions (think format conversion) before uploading
         */
        public bool HasFileField(string fieldName)
        {
            foreach (ValuePair v in files) {
                if (v.name == fieldName) { return true; }
            }
            return false;
        }

        /**
         * Encode parameters 
         * Based on the code by Steven Cheng, http://bytes.com/forum/thread268661.html
         */
        public RequestParameters GetEncodedPostData()
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            Stream memStream = new System.IO.MemoryStream();
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            string formdataTemplate = "\r\n--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";
            foreach (ValuePair v in values)
            {
                string formitem = string.Format(formdataTemplate, v.name, v.value);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }
            memStream.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n";

            foreach (ValuePair v in files)
            {
                string filePath;

                if (overloadedFiles.ContainsKey(v.name))
                {
                    filePath = overloadedFiles[v.name];
                }
                else
                {
                    if (v.value.Length == 0) { continue; } // no file
                    filePath = v.value;
                }

                try // file can be absent or not readable
                { 
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                    string header = string.Format(headerTemplate, v.name, filePath);
                    byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                    memStream.Write(headerbytes, 0, headerbytes.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        memStream.Write(buffer, 0, bytesRead);
                    }

                    memStream.Write(boundarybytes, 0, boundarybytes.Length);
                    fileStream.Close();
                }
                catch (Exception x) // no file?..
                {
                    MessageBox.Show(x.Message, "Cannot upload the file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            RequestParameters result = new RequestParameters();

            memStream.Position = 0;
            result.data = new byte[memStream.Length];
            memStream.Read(result.data, 0, result.data.Length);
            memStream.Close();

            result.headers = "Content-Type: multipart/form-data; boundary=" + boundary + "\r\n" +
                             "Content-Length: " + result.data.Length + "\r\n" +
                             "\r\n";

            return result;
        }
    }
}