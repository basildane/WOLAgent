using System.ServiceProcess;

namespace Agent
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new WOL_Agent() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
