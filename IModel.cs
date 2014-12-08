using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal interface IModel
    {
        [Key]
        int id { get; set; }

        [Key]
        int other { get; set; }

        [Index("toto")]
        string toto { get; set; }
    }
}
