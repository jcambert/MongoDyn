using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public class DocumentException:Exception
    {
        public DocumentException(string message):base()
        {

        }
    }
}
