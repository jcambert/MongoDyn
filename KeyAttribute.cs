using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false,Inherited=true)]
    public sealed class KeyAttribute:Attribute
    {
    }
}
