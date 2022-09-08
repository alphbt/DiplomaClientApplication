using Microsoft.Extensions.Configuration;
using DiplomaClientApplication.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaClientApplication
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IConfigurationRoot Configuration { get; set; }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            //Console.WriteLine("OnStartUp Start");
            var builder = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile(@$"appconfig.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            //Console.WriteLine("Try To Create MainWindow");

            try
            {
                //Console.Write("In Try Area");
                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                //Console.WriteLine("MainWindow Created");
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //finally
            //{
            //    Console.WriteLine("OnStartUp Finish");
            //}

        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped(typeof(MainWindow));

            services.AddOptions();

            services.Configure<MyConfig>(Configuration.GetSection("Config"));
        }
    }
}
