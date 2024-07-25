using System;
using System.Windows.Forms;

namespace c_sharp
{
    static class Program
    {
        public static Form1 form = new Form1();

        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();            

            form.FormLayout();;
            Application.Run(form);
        }
    }
}
