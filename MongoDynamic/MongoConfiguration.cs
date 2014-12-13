using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public class MongoConfiguration
    {

        static Configuration configuration_;

        static MongoConfiguration()
        {
            configuration_= ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
        }
        public static MongoSection Section
        {
            get
            {
                 var  section = ConfigurationManager.GetSection("mongodyn/mongo") ;
                
                 return (MongoSection)section;
            }
        }

        public static Configuration Configuration
        {
            get
            {
                return configuration_;
            }
        }
        

    }
}
