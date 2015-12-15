using System;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;

using Miqi.Net;

namespace Miqi
{
    class Program
    {
		/*
        static void Main()
        {
            MiqiServer server = new MiqiServer(54321);
            server.Start();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            server.Stop();
        }
		*/
		
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm theForm = new MainForm();
            Application.Run(theForm);
        }
    }
}
