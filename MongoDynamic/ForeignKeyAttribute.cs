using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
    public enum RelationShip
    {
        Embeddded,
        Reference
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(RelationShip relation)
            : this(string.Empty, RelationShip.Embeddded)
        {

        }
        public ForeignKeyAttribute(string foreignKeyName):this(foreignKeyName, RelationShip.Embeddded)
        {

        }

        public ForeignKeyAttribute(string foreignKeyName,RelationShip relation)
        {
            this.ForeignKeyName = foreignKeyName;
            this.Relation = relation;
        }

        public string ForeignKeyName { get; private set; }

        public RelationShip Relation { get; private set; }


        public bool HasForeign
        {
            get
            {
                return string.IsNullOrEmpty(ForeignKeyName);
            }
        }
       
    }
}
