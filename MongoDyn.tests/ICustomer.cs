using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDyn.tests
{
    public interface ICustomer
    {
        [Key]
        int id { get; set; }

        [Index("name",true)]
        string Name { get; set; }
    }
}
