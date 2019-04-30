using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using System.Dynamic;
using ProyectoMicroSQL.Models;
using ProyectoMicroSQL.Controllers;

namespace ProyectoMicroSQL.Controllers
{
    public class MicroSQLController : Controller
    {

        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult Configuracion()
        {           
            return RedirectToAction("ConfiguracionDiccionarioManual");
        }
        [HttpPost]
        public ActionResult ImportarArchivo(HttpPostedFileBase Post)
        {
            Datos.Instance.diccionarioColeccionada.Clear();
            string archivoConseguidas = string.Empty;
            if (Post != null)
            {
                string ArchivoEstampas = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(ArchivoEstampas))
                {
                    Directory.CreateDirectory(ArchivoEstampas);
                }                
                archivoConseguidas = ArchivoEstampas + Path.GetFileName(Post.FileName);
                string extension = Path.GetExtension(Post.FileName);
                Post.SaveAs(archivoConseguidas);
                string csvData = System.IO.File.ReadAllText(archivoConseguidas);
                foreach (string fila in csvData.Split('\n'))
                {
                    if (!string.IsNullOrEmpty(fila))
                    {
                        String[] campos = fila.Split(',');
                        string Llave = campos[0];
                        string Identificador = campos[1];
                        Datos.Instance.diccionarioColeccionada.Add(Llave, Identificador);
                    }
                }                
            }           
            return RedirectToAction("IngresarSQL");
        }
        public ActionResult ConfiguracionDiccionarioManual()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfiguracionDiccionarioManual(FormCollection collection)
        {
            var DiccionarioVar = new Diccionario
            {
                FuncionCreateTable = collection["FuncionCreateTable"],
                FuncionDelete = collection["FuncionDelete"],
                FuncionDropTable = collection["FuncionDropTable"],
                FuncionFrom = collection["FuncionFrom"],
                FuncionGo = collection["FuncionGo"],
                FuncionInsertInto = collection["FuncionInsertInto"],
                FuncionSelect = collection["FuncionSelect"],
                FuncionValue = collection["FuncionValue"],
                FuncionWhere = collection["FuncionWhere"]
            };
            Datos.Instance.diccionarioColeccionada.Clear();
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionCreateTable,"CREATE TABLE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDelete, "DELETE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDropTable, "DROP TABLE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionFrom, "FROM");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionGo, "Go");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionInsertInto, "INSERT INTO");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionSelect, "SELECT");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionValue, "VALUE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionWhere, "WHERE");
            return RedirectToAction("IngresarSQL");
        }

        public ActionResult ConfiguracionDiccionarioAuto()
        {
            string csvData = System.IO.File.ReadAllText(Server.MapPath(@"~/App_Data/DeafultDefinition.csv"));

            foreach (string fila in csvData.Split('\n'))
            {
                if (!string.IsNullOrEmpty(fila))
                {
                    String[] campos = fila.Split(',');
                    string Llave = campos[0];
                    string Identificador = campos[1];
                    Datos.Instance.diccionarioColeccionada.Add(Llave, Identificador);
                }
            }
            return RedirectToAction("Menu");
        }
        public ActionResult IngresarSQL()
        {
            return RedirectToAction("Data");
        }
        public ActionResult Data()
        {
            return View("DatosSQL");
        }
        [HttpPost]
        public ActionResult Data(FormCollection Sintaxis)
        {
            //foreach (var item in )
            //{



            //}
            return View("DatosSQL");
        }
    }
}
//public void Insertar(string [] listaComandos)
//{
//    //Logic here...
//    return;
//}