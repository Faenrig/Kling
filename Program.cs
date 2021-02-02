using System;
using System.Threading;
using System.Windows.Forms;

namespace Kling
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexName = System
                .Reflection
                .Assembly
                .GetExecutingAssembly()
                .GetType()
                .GUID.ToString();

            using (Mutex mutex = new Mutex(false, mutexName, out bool createdNew))
            {
                if (!createdNew)
                {
                    // Only allow one instance
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                try
                {
                    Context context = new Context();
                    Application.Run(context);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message+Environment.NewLine+exc.StackTrace, "Error");
                }
            }
        }
    }
}
