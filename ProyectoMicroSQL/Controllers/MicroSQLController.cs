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
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionCreateTable, "CREATE TABLE");
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
            Datos.Instance.ListaAtributos.Add("PRIMARY KEY");
            Datos.Instance.ListaAtributos.Add("VARCHAR(100)");
            Datos.Instance.ListaAtributos.Add("INT");
            Datos.Instance.ListaAtributos.Add("DATETIME");
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
            var OperacionSintaxis = new Sintaxis
            {
                SintaxisEscrita = Sintaxis["SintaxisEscrita"],
            };
            string DelimitadorGO = Datos.Instance.diccionarioColeccionada.ElementAt(8).Key;
            string[] ArregloOperaciones = Regex.Split(OperacionSintaxis.SintaxisEscrita,DelimitadorGO);
            for (int i = 0; i < ArregloOperaciones.Length; i++)//Se guarda en una linea
            {
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], @"\r\n?|\n","").Trim();//Se concatena
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], "  ", " ").Trim();//Se quitan las lineas
            }
            for (int i = 0; i < ArregloOperaciones.Length; i++)
            {
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key))
                {
                    //Crear Tabla   
                    CrearTabla(ArregloOperaciones[i]);
                }
            }
            return View("DatosSQL");
        }
        public void CrearTabla(string Valor)
        {
            string NombreTabla = "";
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key, "").Trim();
            string[] SepararParentesis = Valor.Split(new char[] { '(' }, 2);
            if (SepararParentesis.Length == 1)
            {
                throw new System.InvalidOperationException("No contiene parentesis de apertura");
            }
            NombreTabla = SepararParentesis[0].Trim();
            if (!Valor.Contains(Datos.Instance.ListaAtributos.ElementAt(0)))
            {
                throw new System.InvalidOperationException("No contiene " + Datos.Instance.ListaAtributos.ElementAt(0));
            }
            SepararParentesis[1] = SepararParentesis[1].Trim();
            if (!SepararParentesis[1][SepararParentesis[1].Length-1].Equals(')'))
            {
                throw new System.InvalidOperationException("No contiene parentesis de clausura");
            }
            SepararParentesis[1] = SepararParentesis[1].Substring(0, SepararParentesis[1].Length - 2);
            string[] ValoresTabla = SepararParentesis[1].Split(',');
            if (ValoresTabla.Any(x => x.Trim() == ""))
            {
                throw new System.InvalidOperationException("Tiene error en las comas de separación de las columnas");
            }
            for (int i = 0; i < ValoresTabla.Length; i++)
            {
                ValoresTabla[i] = ValoresTabla[i].Trim();
            }

            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(1))).Count < 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(1));
            }
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(2))).Count < 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(2));
            }
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(3))).Count < 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(3));
            }

            var TamanoKey = Datos.Instance.ListaAtributos.ElementAt(0).Trim().Split(' ').Length;
            if (ValoresTabla[0].Split(' ').Length != 2 + TamanoKey)
            {
                throw new System.InvalidOperationException(Datos.Instance.ListaAtributos.ElementAt(4) + " debe de contener 3 campos[NOMBRE TIPO LLAVE]");
            }
            if (ValoresTabla[0].Split(' ')[0].ToUpper() != "ID")
            {
                throw new System.InvalidOperationException(Datos.Instance.ListaAtributos.ElementAt(4) + " debe contener ID como primer campo");
            }
            if (ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(0) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(1) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(2) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(3))
            {
                throw new System.InvalidOperationException("No puede ser un tipo de dato o un " + Datos.Instance.ListaAtributos.ElementAt(0));
            }
            if (ValoresTabla[0].Split(' ')[1] != Datos.Instance.ListaAtributos.ElementAt(2))
            {
                throw new System.InvalidOperationException("Debe ser un tipo " + Datos.Instance.ListaAtributos.ElementAt(2));
            }
            string LlavePrimaria = " ";
            for (int i = 0; i < ValoresTabla[0].Split(' ').Length; i++)
            {
                LlavePrimaria += ValoresTabla[0].Split(' ')[i] + " ";
            }
            LlavePrimaria = LlavePrimaria.Trim();
            if (LlavePrimaria != Datos.Instance.ListaAtributos.ElementAt(0))
            {
                throw new System.InvalidOperationException("Debe ser " + Datos.Instance.ListaAtributos.ElementAt(0));
            }

            List<string> ListaNombreColumna = new List<string>();
            List<string> ListaTipoColumnas = new List<string>();
            string[] ArregloNombreColumnas = new string[9];
            ArregloNombreColumnas[0] = "ID";
            ListaNombreColumna.Add("ID");
            ListaTipoColumnas.Add("INT");
            for (int i = 1; i < ValoresTabla.Length; i++)
            {
                if (ValoresTabla[i].Split(' ').Length != 2)
                {
                    throw new System.InvalidOperationException("Los atributos que no son PRIMARY KEY no deben de tener mas de dos campos");
                }
                if (ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(0) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(1) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(2) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(3))
                {
                    throw new System.InvalidOperationException("Los atributos que no son PRIMARY KEY deben contener el nombre en el campo 1");
                }
                if (ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(1) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(2) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(3))
                {
                    throw new System.InvalidOperationException("Los atributos que no son PRIMARY KEY deben contener el tipo en el campo 2");
                }
                if (ArregloNombreColumnas.Any(x => ValoresTabla[i].Split(' ')[0].ToUpper() == x))
                {
                    throw new System.InvalidOperationException("No se pueden repetir nombres de columnas");
                }
                ArregloNombreColumnas[i] = ValoresTabla[i].Split(' ')[0].ToUpper();
                ListaNombreColumna.Add(ValoresTabla[i].Split(' ')[0].ToUpper());
                ListaTipoColumnas.Add(ValoresTabla[i].Split(' ')[0].ToUpper());
            }
        }
    }

}
//public void Insertar(string [] listaComandos)
//{
//    //Logic here...
//    return;
//}


