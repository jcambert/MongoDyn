using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    [AttributeUsage(AttributeTargets.Property,  Inherited = true)]
    public sealed class IndexAttribute:Attribute
    {
        public IndexAttribute(string name):this(name,true)
        {

        }

        public IndexAttribute(string name,bool isUnique)
        {
            this.Name = name;
            this.IsUnique = isUnique;

        }

        public string Name { get; private set ; }

        public bool IsUnique { get; private set; }
    }
}
