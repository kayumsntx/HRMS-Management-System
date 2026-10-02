using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Common.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(string name, object key)
       : base($"{name} (Id: {key}) Not Found.") { }

        public NotFoundException(string message) : base(message) { }
    }
}
