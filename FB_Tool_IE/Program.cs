using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace KSTN_Facebook_Tool
{
    static class Program
    {
        public static MainForm mainForm;
        public static WebBrowserForm wbf;
        public static LoadingForm loadingForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool successAquisition;
            Mutex programMutex = new Mutex(true,
                AppDomain.CurrentDomain.SetupInformation.ApplicationName,
                out successAquisition);

            if (successAquisition)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                mainForm = new MainForm();
                Application.Run(mainForm);
            }
            else
            {
                MessageBox.Show("Ứng dụng đang được chạy! Bạn chỉ có thể mở 01 cửa sổ chương trình!", "Ứng dụng đang chạy");
            }
        }
    }
}
