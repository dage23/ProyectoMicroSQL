using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbolBDLL
{
    class GUID
    {
        private static GUID instancia;
        private static GUID obj;

        public static GUID getInstancia()
        {
            if (instancia == null)
            {
                instancia = new GUID();
            }

            return instancia;
        }

        public string GenerateGuid()
        {
            obj = new GUID();
            return obj.ToString();
        }
    }
}
