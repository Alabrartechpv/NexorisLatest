using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.Master
{
  public  class CategoryModel
    {
    }
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int GroupId { set; get; }
        public Byte[] Photo { get; set; }
        public string _Operation { get; set; }
    }
    public class CategoryDDL
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public int GroupId { set; get; }
    }

    public class CategoryDDlGrid
    {
        public IEnumerable<CategoryDDL> List { get; set; }
    }

  

}
