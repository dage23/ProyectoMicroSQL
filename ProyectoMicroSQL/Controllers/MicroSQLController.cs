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
        public ActionResult ConfiguracionDiccionarioManual()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfiguracionDiccionarioManual(HttpPostedFileBase Post)
        {
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
            return RedirectToAction("Menu");
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
        public ActionResult Data(HttpPostedFileBase Post)
        {
            return View("DatosSQL");
        }
    }
}
