using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace ProyectoMicroSQL.Models
{
    public class Diccionario
    {
        [Display(Name = "SELECT")]
        public string FuncionSelect { get; set; }
        [Display(Name = "FROM")]
        public string FuncionFrom { get; set; }
        [Display(Name = "DELETE")]
        public string FuncionDelete { get; set; }
        [Display(Name = "WHERE")]
        public string FuncionWhere { get; set; }
        [Display(Name = "CREATE TABLE")]
        public string FuncionCreateTable { get; set; }
        [Display(Name = "DROP TABLE")]
        public string FuncionDropTable { get; set; }
        [Display(Name = "INSERT INTO")]
        public string FuncionInsertInto { get; set; }
        [Display(Name = "VALUE")]
        public string FuncionValue { get; set; }
        [Display(Name = "GO")]
        public string FuncionGo { get; set; }

    }
}