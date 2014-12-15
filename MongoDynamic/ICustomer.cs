using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDyn
{



    [MongoModel]
    public interface ICustomer
    {
        [Key]
        int Id { get; set; }
        
        [Index("name",true)]
        string Name { get; set; }


        string Phone { get; set; }


       [ForeignKey("Customer", RelationShip.Embeddded)]
        IList<IOrder> Orders { get; set; }

        //No reverse reference
        //[ForeignKey("Saler", RelationShip.Embeddded)]
//        IList<IOrder> Sales { get; set; }

    }

    [MongoModel]
    public interface IOrder
    {
       [Key]
        int Id { get; set; }

        string Description { get; set; }
        
        ICustomer Customer { get; set; }


        ICustomer Saler{get;set;}


    }
}
