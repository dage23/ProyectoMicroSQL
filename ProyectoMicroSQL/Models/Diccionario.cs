using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoMicroSQL.Models
{
    public class Diccionario
    {
        public string FuncionSelect { get; set; }
        public string FuncionFrom { get; set; }
        public string FuncionDelete { get; set; }
        public string FuncionWhere { get; set; }
        public string FuncionCreateTable { get; set; }
        public string FuncionDropTable { get; set; }
        public string FuncionInsertInto { get; set; }
        public string FuncionValue { get; set; }
        public string FuncionGo { get; set; }

    }
}