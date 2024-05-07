using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemB.Generator
{
    public interface IDataGenerator<T>
    {
        public T Next();
    }
}
