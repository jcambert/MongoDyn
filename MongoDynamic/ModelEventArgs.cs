using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDyn
{
   public  class ModelEventArgs<TModel>:EventArgs
    {
       public ModelEventArgs(TModel model)
       {
           this.Model = model;
       }

       public TModel Model { get; private set; }
    }
}
