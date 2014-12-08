using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal class Pair
    {
        public Pair(PropertyInfo property,IndexAttribute attr)
        {
            this.Property = property;
            this.Attribute = attr;

        }

        public PropertyInfo Property { get; private set; }

        public IndexAttribute Attribute { get;private set; }
    }
}
