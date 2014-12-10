
# MongoDynamic


The MongoDynamic project is a library that help you to abstract your poco


## Utilisation :

### Design your poco:
 public interface ICustomer{
    
        [Key]
        int id { get; set; }

        [Index("name",true)]
        string Name { get; set; }
    }

   * set the Key attribute (only one is authorized)
   * set Indexes

### Acces the repository
    DynamicCollection<int, ICustomer> Customers = Dynamic.GetCollection<int, ICustomer>();

### Perform crud
    Customers.DeleteAll(true);
    var customer = Customers.New();
    customer.Name = "CustormerName";
    Customers.Upsert(customer);
    var customer0 = Customers.GetByKey(1);

## More resources:


* [Releases](https://github.com/jcambert/MongoDyn/tree/master/MongoDynamic)

