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
        public string FuncionSelect { get; set; }//0
        [Display(Name = "FROM")]
        public string FuncionFrom { get; set; }//1
        [Display(Name = "DELETE")]
        public string FuncionDelete { get; set; }//2
        [Display(Name = "WHERE")]
        public string FuncionWhere { get; set; }//3
        [Display(Name = "CREATE TABLE")]
        public string FuncionCreateTable { get; set; }//4
        [Display(Name = "DROP TABLE")]
        public string FuncionDropTable { get; set; }//5
        [Display(Name = "INSERT INTO")]
        public string FuncionInsertInto { get; set; }//6
        [Display(Name = "VALUE")]
        public string FuncionValue { get; set; }//7
        [Display(Name = "GO")]
        public string FuncionGo { get; set; }//8

    }
}