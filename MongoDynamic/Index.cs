using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    internal class Index
    {
        readonly List<string> fields = new List<string>();
        public Index(string name,bool isUnique)
        {
            this.Name = name;
            this.IsUnique = isUnique;
        }

        public string Name { get; private set; }

        public bool IsUnique { get;private  set; }

        public void AddField(string name)
        {
            fields.Add(name);

        }

        public void AddFields(List<string> names)
        {
            fields.AddRange(names);

        }

        public List<string> Fields
        {
            get
            {
                return fields;
            }
        }
    }
}
