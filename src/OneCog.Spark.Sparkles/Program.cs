using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace OneCog.Spark.Sparkles
{
    class Program
    {
        static void Main(string[] args)
        {
            IKernel kernel = new StandardKernel(new Module());

            HostFactory.Run(x =>                                 
            {
                x.Service<IService>(s =>                        
                {
                    s.ConstructUsing(name => kernel.Get<IService>());
                    s.WhenStarted(tc => tc.Start());             
                    s.WhenStopped(tc => tc.Stop());               
                });
                x.RunAsLocalSystem();                            

                x.SetDescription("Service for reading values from SparkCore cloud API and writing the values to ElasticSearch");
                x.SetDisplayName("SparklES");                       
                x.SetServiceName("SparklES");                       
            });    
        }
    }
}
