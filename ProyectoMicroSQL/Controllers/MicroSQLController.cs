using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using System.Dynamic;
using ProyectoMicroSQL.Models;

namespace ProyectoMicroSQL.Controllers
{
    public class MicroSQLController : Controller
    {
        public ActionResult ConfiguracionDiccionarioManual()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ConfiguracionDiccionarioManual(HttpPostedFileBase PostConseguidas)
        {
            string archivoConseguidas = string.Empty;
            if (PostConseguidas != null)
            {
                string ArchivoEstampas = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(ArchivoEstampas))
                {
                    Directory.CreateDirectory(ArchivoEstampas);
                }
                archivoConseguidas = ArchivoEstampas + Path.GetFileName(PostConseguidas.FileName);
                string extension = Path.GetExtension(PostConseguidas.FileName);
                PostConseguidas.SaveAs(archivoConseguidas);
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
        [HttpPost]
        public ActionResult ConfiguracionDiccionarioAuto()
        {
            string csvData = System.IO.File.ReadAllText("c:\\...\\ProyectoMicroSQL\\DeafultDefinition.csv");
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
    }
}
