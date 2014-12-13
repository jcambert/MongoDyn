using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace MongoDyn
{
    public class MongoSection : ConfigurationSection
    {
        [ConfigurationProperty("host", DefaultValue = "localhost", IsRequired = false)]
        //[StringValidator(MinLength = 1, MaxLength = 60)]
        public String Host
        {
            get
            {
                return (String)this["host"];
            }
            set
            {
                this["host"] = value;
            }
        }

        [ConfigurationProperty("port", DefaultValue = "27017", IsRequired = false)]
       
        public int Port
        {
            get
            {
                return (int)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }


        [ConfigurationProperty("database", IsRequired = true)]
        //[StringValidator(MinLength = 1, MaxLength = 60)]
        public String Database
        {
            get
            {
                return (String)this["database"];
            }
            set
            {
                this["database"] = value;
            }
        }
    }


}
