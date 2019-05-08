using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ProyectoMicroSQL.Models
{
    public class Diccionario
    {
        public string FuncionSelect { get; set; }//0
        public string FuncionFrom { get; set; }//1
        public string FuncionDelete { get; set; }//2
        public string FuncionWhere { get; set; }//3
        public string FuncionCreateTable { get; set; }//4
        public string FuncionDropTable { get; set; }//5
        public string FuncionInsertInto { get; set; }//6
        public string FuncionValue { get; set; }//7
        public string FuncionGo { get; set; }//8

    }
}