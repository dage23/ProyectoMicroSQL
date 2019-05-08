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
using BTreeDLL;
using Tabla = BTreeDLL.Tabla;

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
        //--------------------------------IMPORTAR ARCHIVO QUE ESCOGE USUARIO-------------------------
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
        //-------------------------------------------OBTENER CAMPOS ESCRITOS POR EL USUARIO---------------------------------------------------
        [HttpPost]
        public ActionResult ConfiguracionDiccionarioManual(FormCollection collection)
        {
            var DiccionarioVar = new Diccionario
            {
                FuncionSelect = collection["FuncionSelect"],
                FuncionFrom = collection["FuncionFrom"],
                FuncionDelete = collection["FuncionDelete"],
                FuncionWhere = collection["FuncionWhere"],
                FuncionCreateTable = collection["FuncionCreateTable"],
                FuncionDropTable = collection["FuncionDropTable"],
                FuncionInsertInto = collection["FuncionInsertInto"],
                FuncionValue = collection["FuncionValue"],
                FuncionGo = collection["FuncionGo"]
            };
            Datos.Instance.diccionarioColeccionada.Clear();
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionSelect, "SELECT");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionFrom, "FROM");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDelete, "DELETE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionWhere, "WHERE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionCreateTable, "CREATE TABLE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDropTable, "DROP TABLE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionInsertInto, "INSERT INTO");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionValue, "VALUE");
            Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionGo, "GO");
            return RedirectToAction("IngresarSQL");
        }
        //--------------------------------------------------------------IMPORTAR AUTOMATICAMENTE LAS PALABRAS RESERVADAS---------------------------------
        public ActionResult ConfiguracionDiccionarioAuto()
        {
            string csvData = System.IO.File.ReadAllText(Server.MapPath(@"~/MicroSQL/microSQL.ini"));

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
        //-------------------------------------------------ACCEDER A INSTRUCCIONES DE USUARIO------------------------------------
        [HttpPost]
        public ActionResult Data(FormCollection Sintaxis)
        {
            var OperacionSintaxis = new Sintaxis
            {
                SintaxisEscrita = Sintaxis["SintaxisEscrita"],
            };
            string DelimitadorGO = Datos.Instance.diccionarioColeccionada.ElementAt(8).Key;
            string[] ArregloOperaciones = Regex.Split(OperacionSintaxis.SintaxisEscrita, DelimitadorGO);
            for (int i = 0; i < ArregloOperaciones.Length; i++)//Se guarda en una linea
            {
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], @"\r\n?|\n", "").Trim();//Se concatena
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], "  ", " ").Trim();//Se quitan las lineas
            }
            for (int i = 0; i < ArregloOperaciones.Length; i++)
            {
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key))
                {
                    //Crear Tabla   
                    CrearTablaYArbol(ArregloOperaciones[i]);
                }
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key))
                {
                    //Insertar en Tabla   
                    InsertarEnTablaArbol(ArregloOperaciones[i]);
                }
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key))
                {
                    //Seleccionar de Tabla   
                    SeleccionarDatosParaMostrar(ArregloOperaciones[i]);
                }

            }
            return View("DatosSQL");
        }
        public ActionResult VerTablas()
        {
            return View(Datos.Instance.ListaTablaYValores);
        }
        //-------------------------------CREATE TABLE--------------------------
        private void CrearTablaYArbol(string Valor)
        {
            string NombreTabla = "";
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key, "").Trim();
            string[] SepararParentesis = Valor.Split(new char[] { '(' }, 2);
            //-----------------------------------------Errores de sintaxis----------------------
            NombreTabla = SepararParentesis[0].Trim();
            if (SepararParentesis.Length == 1)
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " no contiene parentesis de apertura");
            }
            if (!Valor.Contains(Datos.Instance.ListaAtributos.ElementAt(0)))
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " no contiene " + Datos.Instance.ListaAtributos.ElementAt(0));
            }
            SepararParentesis[1] = SepararParentesis[1].Trim();
            if (!SepararParentesis[1][SepararParentesis[1].Length - 1].Equals(')'))
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " no contiene parentesis de clausura");
            }
            SepararParentesis[1] = SepararParentesis[1].Substring(0, SepararParentesis[1].Length - 2);
            string[] ValoresTabla = SepararParentesis[1].Split(',');
            if (ValoresTabla.Length < 2)
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " no puede insertar solo 1 atributo");
            }
            if (ValoresTabla.Any(x => x.Trim() == ""))
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " tiene error en las comas de separación de las columnas");
            }
            for (int i = 0; i < ValoresTabla.Length; i++)
            {
                ValoresTabla[i] = ValoresTabla[i].Trim();
            }
            //-----------------------------Errores tipos de datos--------------------
            //Mas de 3 varchar
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(1))).Count >= 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(1));
            }
            //Mas de 3 int
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(2))).Count >= 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(2));
            }
            //Mas de 3 datetime
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(3))).Count >= 3)
            {
                throw new System.InvalidOperationException("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(3));
            }
            //--------------------------------Operaciones con Primary key---------------------------------
            var TamanoKey = Datos.Instance.ListaAtributos.ElementAt(0).Trim().Split(' ').Length;
            //--------------------------------Errores--------------------------------------------------
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
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " utiliza un atributo no permitido, debe de ser tipo " + Datos.Instance.ListaAtributos.ElementAt(0));
            }
            if (ValoresTabla[0].Split(' ')[1] != Datos.Instance.ListaAtributos.ElementAt(2))
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + ", el primer dato debe ser un tipo " + Datos.Instance.ListaAtributos.ElementAt(2));
            }
            //--------------------------------------TIpo de llave primaria-------------------------------
            string LlavePrimaria = " ";
            for (int i = 2; i < ValoresTabla[0].Split(' ').Length; i++)
            {
                LlavePrimaria += ValoresTabla[0].Split(' ')[i] + " ";
            }
            LlavePrimaria = LlavePrimaria.Trim();
            if (LlavePrimaria != Datos.Instance.ListaAtributos.ElementAt(0))
            {
                throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + ", el primer dato debe ser un " + Datos.Instance.ListaAtributos.ElementAt(0));
            }
            //----------------------------------------Creacion de archivos------------------------------
            List<string> ListaNombreColumna = new List<string>();
            List<string> ListaTipoColumnas = new List<string>();
            string[] ArregloNombreColumnas = new string[9];
            ArregloNombreColumnas[0] = "ID";
            ListaNombreColumna.Add("ID");
            ListaTipoColumnas.Add("INT");
            for (int i = 1; i < ValoresTabla.Length; i++)
            {
                //--------------------------------------Errores de atributos--------------------
                if (ValoresTabla[i].Split(' ').Length != 2)
                {
                    throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " los atributos que no son PRIMARY KEY no deben de tener mas de dos campos");
                }

                if (ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(0) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(1) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(2) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(3))
                {
                    throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " los atributos que no son PRIMARY KEY deben contener el nombre en el campo 1");
                }
                if (ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(1) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(2) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(3))
                {
                    throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + ", los atributos que no son PRIMARY KEY deben contener el tipo en el campo 2");
                }
                if (ArregloNombreColumnas.Any(x => ValoresTabla[i].Split(' ')[0].ToUpper() == x))
                {
                    throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + ", no se pueden repetir nombres de columnas");
                }
                ArregloNombreColumnas[i] = ValoresTabla[i].Split(' ')[0].ToUpper();
                ListaNombreColumna.Add(ValoresTabla[i].Split(' ')[0].ToUpper());
                ListaTipoColumnas.Add(ValoresTabla[i].Split(' ')[1].ToUpper());
            }
            string[] ArregloListaNombreTablas = Datos.Instance.ListaTablasExistentes.ToArray();
            if (Datos.Instance.ListaTablasExistentes.Count() > 0)
            {
                for (int i = 0; i < ArregloListaNombreTablas.Length; i++)
                {
                    if (ArregloListaNombreTablas[i] == NombreTabla.ToUpper())
                    {
                        throw new System.InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + ", el nombre " + NombreTabla + " no puede repetirse");
                    }
                }
            }

            //-------------------------------Crear archivo.tabla------------------
            CrearArchivoTabla(ListaNombreColumna, ListaTipoColumnas, NombreTabla);
            //-------------------------------Crear archivo.arbolb-----------------
            CrearArbolDeTabla(NombreTabla);
            //-------------------------------Agregar a lista----------------------
            string ValoresdeTabla = "";
            for (int i = 0; i < ListaNombreColumna.Count; i++)
            {
                if (i == ListaNombreColumna.Count - 1)
                {
                    ValoresdeTabla += ListaNombreColumna[i] + ", " + ListaTipoColumnas[i];
                }
                else
                {
                    ValoresdeTabla += ListaNombreColumna[i] + ", " + ListaTipoColumnas[i] + "~~";
                }
            }
            Datos.Instance.ListaTablaYValores.Add(new Listado_Tablas { NombreTabla = NombreTabla, ValoresTabla = ValoresdeTabla });

        }
        //-------------------------------Crear archivo.tabla-----------
        private void CrearArchivoTabla(List<string> NombreColumnas, List<string> TipoColumnas, string Nombre)
        {
            FileStream ArchivoTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + Nombre + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter Escritor = new StreamWriter(ArchivoTabla);
            Escritor.WriteLine(Nombre);
            for (int i = 0; i < NombreColumnas.Count; i++)
            {
                if (i == NombreColumnas.Count - 1)
                {
                    Escritor.Write(NombreColumnas.ElementAt(i) + "|" + TipoColumnas.ElementAt(i));
                }
                else
                {
                    Escritor.WriteLine(NombreColumnas.ElementAt(i) + "|" + TipoColumnas.ElementAt(i));
                }
            }
            Datos.Instance.ListaTablasExistentes.Add(Nombre);
            Escritor.Flush();
            ArchivoTabla.Close();
        }
        //-------------------------------Crear archivo.arbolb-----------
        private void CrearArbolDeTabla(string NombreArbol)
        {
            BTreeDLL.BTree<string, BTreeDLL.Tabla> CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arboles/" + NombreArbol + ".arbolb"), 8);
            CrearArbol.CloseStream();
        }
        //-------------------------------INSERT INTO----------------------------------------
        private void InsertarEnTablaArbol(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key, "").Trim();
            if (Valor.Count(x => x == '(') != 2 || Valor.Count(y => y == ')') != 2)
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " necesitas revisar la sintaxis de tu codigo (PARENTESIS)");
            }
            string[] SeparadorComas = Valor.Split(new char[] { '(' }, 2);
            SeparadorComas[0] = SeparadorComas[0].Trim();
            if (SeparadorComas[0].Split(' ').Length > 1)
            {
                throw new InvalidOperationException("El nombre de la tabla no puede ser mayor de dos campos");
            }
            string NombreTablaInsertar = SeparadorComas[0];
            bool TablaEncontradaEnLista = false;
            for (int i = 0; i < Datos.Instance.ListaTablasExistentes.Count(); i++)
            {
                if ((NombreTablaInsertar.ToUpper() == Datos.Instance.ListaTablasExistentes[i]))
                {
                    TablaEncontradaEnLista = true;
                    break;
                }
            }
            if (!TablaEncontradaEnLista)
            {
                throw new InvalidOperationException("El nombre " + NombreTablaInsertar + " no existe en ninguna tabla");
            }
            FileStream TablaEnArhivo = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTablaInsertar + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader Lector = new StreamReader(TablaEnArhivo);
            List<string> ColumnaEnArchivo = new List<string>();
            List<string> TipoEnArchivo = new List<string>();
            string TipoActual;
            Lector.ReadLine();
            while ((TipoActual = Lector.ReadLine()) != null)
            {
                ColumnaEnArchivo.Add(TipoActual.Split('|')[0]);
                TipoEnArchivo.Add(TipoActual.Split('|')[1]);
            }
            TablaEnArhivo.Close();

            string[] SepararPorPrimerParentesis = SeparadorComas[1].Trim().Split(new char[] { ')' }, 2);
            if (SepararPorPrimerParentesis[0].Trim().Split(',').Length != ColumnaEnArchivo.Count)
            {
                throw new InvalidOperationException("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + ", el numero de atributos de atributos no concuerda con los de la tabla");
            }

            string[] ColumnaInstruccion = new string[ColumnaEnArchivo.Count];
            ColumnaInstruccion = SepararPorPrimerParentesis[0].Trim().Split(',');

            for (int i = 0; i < ColumnaInstruccion.Length; i++)
            {
                if (ColumnaInstruccion[i].Trim().ToUpper() != ColumnaEnArchivo.ElementAt(i))
                {
                    throw new InvalidOperationException("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + ", no coinciden los atributos de la tabla con los almacenados");
                }
            }

            string[] SepararPorSegundoParentesis = SepararPorPrimerParentesis[1].Trim().Split(new char[] { '(' }, 2);
            if (SepararPorSegundoParentesis[0].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(7).Key)
            {
                throw new InvalidOperationException("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + "no posee" + Datos.Instance.diccionarioColeccionada.ElementAt(7).Key);
            }

            string[] SepararPorTercerParentesis = SepararPorSegundoParentesis[1].Trim().Split(new char[] { ')' }, 2);
            if (SepararPorTercerParentesis[1].Trim() != "")
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " tiene un error en los parentesis");
            }

            if (SepararPorTercerParentesis[0].Trim().Split(',').Length != TipoEnArchivo.Count)
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " no introduce la cantidad de valores requeridos");
            }


            List<object> ListaObjetosAInsertar = new List<object>();
            string TipoDatoInsertar = "";
            int IDInsertar = 0;
            bool TipoDatoExiste = false;

            string[] ArregloDAtosInsertar = SepararPorTercerParentesis[0].Trim().Split(',');
            for (int i = 0; i < ArregloDAtosInsertar.Length; i++)
            {
                TipoDatoExiste = false;
                ArregloDAtosInsertar[i] = ArregloDAtosInsertar[i].Trim();
                TipoDatoInsertar = "";
                try
                {
                    //Convertir int
                    int PruebaConvertirInt = int.Parse(ArregloDAtosInsertar[i]);
                    TipoDatoInsertar = "INT";
                    TipoDatoExiste = true;
                    if (i == 0)
                    {
                        IDInsertar = PruebaConvertirInt;
                    }
                    else
                    {
                        ListaObjetosAInsertar.Add(PruebaConvertirInt);
                    }
                }
                catch (FormatException)
                {
                    //No es int
                }

                if (ArregloDAtosInsertar[i].Length > 2 && !TipoDatoExiste)
                {
                    if (ArregloDAtosInsertar[i][0] == '\'' && ArregloDAtosInsertar[i][ArregloDAtosInsertar[i].Length - 1] == '\'')
                    {
                        ArregloDAtosInsertar[i] = ArregloDAtosInsertar[i].Replace("'", "");
                        if (ArregloDAtosInsertar[i].Trim().Length == 0)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " no permite ingresar datos nulos");
                        }
                        try
                        {
                            //Convertir datetime
                            DateTime PruebaConvertirDateTime = DateTime.Parse(ArregloDAtosInsertar[i]);
                            ListaObjetosAInsertar.Add(PruebaConvertirDateTime);
                            TipoDatoInsertar = "DATETIME";
                            TipoDatoExiste = true;
                        }
                        catch (FormatException)
                        {
                            //convertir varchar(100)
                            TipoDatoInsertar = "VARCHAR(100)";
                            TipoDatoExiste = true;
                            if (ArregloDAtosInsertar[i].Length > 100)
                            {
                                throw new InvalidOperationException("En " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " no puedes insertar VARCHAR mayores a 100");
                            }
                            ListaObjetosAInsertar.Add(ArregloDAtosInsertar[i]);
                        }
                    }
                }
                if (!TipoDatoExiste)
                {
                    throw new InvalidOperationException("En " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + " no se reconoce un tipo de dato a insertar");
                }
                if (TipoDatoInsertar != TipoEnArchivo.ElementAt(i))
                {
                    throw new InvalidOperationException("En " + Datos.Instance.diccionarioColeccionada.ElementAt(6).Key + ", hay un campo que no concuerda con la tabla");
                }
            }
            //Si todo esta bien
            BTreeDLL.Tabla TablaAInsertarEnArbol = new BTreeDLL.Tabla(IDInsertar, ListaObjetosAInsertar);
            BTreeDLL.BTree<string, BTreeDLL.Tabla> ArbolACrear = new BTree<string, Tabla>(Server.MapPath(@"~/microSQL/arboles/" + NombreTablaInsertar + ".arbolb"), 8);

            ArbolACrear.AddElement(TablaAInsertarEnArbol);
            ArbolACrear.CloseStream();
        }
        //-------------------------------SELECT-------------------------------------------
        private void SeleccionarDatosParaMostrar(string Instucciones)
        {
            if (!Instucciones.Contains(Datos.Instance.diccionarioColeccionada.ElementAt(1).Key))
            {
                throw new InvalidOperationException("El metodo " + Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " no contiene" + Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
            }
            Instucciones = Instucciones.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key, "").Trim();
            List<string> ColumnaEnArchivo;
            List<string> TipoDatoEnArchivo;
            if (Instucciones.Trim().Split(' ')[0] == "*")
            {
                //--------------------------------select*from metodo1---------------
                if (Instucciones.Trim().Split(' ')[1] != Datos.Instance.diccionarioColeccionada.ElementAt(1).Key)
                {
                    throw new InvalidOperationException("Posee un error de sintaxis en el metodo " + Datos.Instance.diccionarioColeccionada.ElementAt(0).Key);
                }
                string[] InstuccionesSeparadas = Regex.Split(Instucciones, Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
                bool ExisteTabla = false;
                string NombreDeTabla = InstuccionesSeparadas[1].Trim().Split(' ')[0];
                for (int i = 0; i < Datos.Instance.ListaTablasExistentes.Count; i++)
                {
                    if (Datos.Instance.ListaTablasExistentes[i] == NombreDeTabla.ToUpper())
                    {
                        ExisteTabla = true;
                        break;
                    }
                }
                if (!ExisteTabla)
                {
                    throw new InvalidOperationException("En el comando " + Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " declara una tabla inexistente");
                }
                //Posee escrito where
                if (InstuccionesSeparadas[1].Trim().Split(' ').Length > 1)
                {
                    //---------------------Errores sintaxis-------------------------
                    if (InstuccionesSeparadas[1].Trim().Split(' ')[1] != Datos.Instance.diccionarioColeccionada.ElementAt(3).Key)
                    {
                        throw new InvalidOperationException("Posee un error de escritura con la funcion " + Datos.Instance.diccionarioColeccionada.ElementAt(3).Key);
                    }
                    string[] InstruccionesSeparadas2 = Regex.Split(InstuccionesSeparadas[1].Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(3).Key);
                    InstruccionesSeparadas2[1] = InstruccionesSeparadas2[1].Trim();

                    string[] InstruccionesSeparadas3 = Regex.Split(InstruccionesSeparadas2[1].Trim(), "=");
                    if (InstruccionesSeparadas3.Length != 2)
                    {
                        throw new InvalidOperationException("El comando " + Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee error de escritura en condicion");
                    }

                    if (InstruccionesSeparadas3[1].Trim() == "")
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee un error en la condicion");
                    }

                    if (InstruccionesSeparadas3[0].Trim() == "")
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee un error en la condicion");
                    }
                    //----------------------Errores en Where------------------------
                    string NombreColumnaBuscar = InstruccionesSeparadas3[0].Trim();
                    int ConteoFinal = 0;
                    int Contador = 0;
                    bool EsVarChar = false;
                    if (InstruccionesSeparadas3[1].Trim()[0] == '\'')
                    {
                        EsVarChar = true;
                        for (int i = 0; i < InstruccionesSeparadas3[1].Trim().Length; i++)
                        {
                            if (InstruccionesSeparadas3[1].Trim()[i] == '\'')
                            {
                                Contador++;
                                if (Contador > 1)
                                {
                                    ConteoFinal = i;
                                    break;
                                }
                            }
                        }
                        if (ConteoFinal < 2)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " busca un dato null");
                        }
                        if (InstruccionesSeparadas3[1].Trim().Length != InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1).Length)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee valores extras, debe de ser COLUMNA = DATO");
                        }
                        InstruccionesSeparadas3[1] = InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1);
                    }
                    string DatoABuscar = "";
                    if (EsVarChar)
                    {
                        DatoABuscar = InstruccionesSeparadas3[1].Trim();
                    }
                    else
                    {
                        DatoABuscar = InstruccionesSeparadas3[1].Split(' ')[0];
                    }
                    string TipoDatoABuscar = "";
                    //------------------------Existe Columna------------------
                    FileStream ArchivoDeTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreDeTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    StreamReader LectorTabla = new StreamReader(ArchivoDeTabla);
                    ColumnaEnArchivo = new List<string>();
                    TipoDatoEnArchivo = new List<string>();
                    string TipoActualDato;
                    LectorTabla.ReadLine();
                    while ((TipoActualDato = LectorTabla.ReadLine()) != null)
                    {
                        ColumnaEnArchivo.Add(TipoActualDato.Split('|')[0]);
                        TipoDatoEnArchivo.Add(TipoActualDato.Split('|')[1]);
                    }
                    ArchivoDeTabla.Close();
                    bool ExisteColumna = false;
                    string TipoColumnaEnArchivo = "";
                    int IDColumna = -1;

                    for (int i = 0; i < ColumnaEnArchivo.Count; i++)
                    {
                        if (ColumnaEnArchivo.ElementAt(i) == NombreColumnaBuscar.ToUpper())
                        {
                            TipoColumnaEnArchivo = TipoDatoEnArchivo.ElementAt(i);
                            IDColumna = i;
                            ExisteColumna = true;
                        }
                    }

                    if (!ExisteColumna)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " utiliza el nombre de columna inexistente");
                    }
                    //---------------Existe Tipo Dato---------------------
                    bool ExisteTipoDato = false;
                    string TipodeDato = "";
                    try
                    {
                        //Es int
                        int PruebaConversionInt = int.Parse(DatoABuscar);
                        TipodeDato = "INT";
                        ExisteTipoDato = true;
                    }
                    catch (FormatException)
                    {
                        //No es int
                    }
                    //Datetime o varchar
                    if (DatoABuscar.Length > 2 && !ExisteTipoDato)
                    {
                        if (DatoABuscar[0] == '\'' && DatoABuscar[DatoABuscar.Length - 1] == '\'')
                        {
                            DatoABuscar = DatoABuscar.Replace("'", "");
                            try
                            {
                                //Es datetime
                                DateTime PruebaConversionDatetime = DateTime.Parse(DatoABuscar);
                                TipodeDato = "DATETIME";
                                ExisteTipoDato = true;
                            }
                            catch (FormatException)
                            {
                                //Es varchar
                                TipodeDato = "VARCHAR(100)";
                                ExisteTipoDato = true;
                                if (DatoABuscar.Length > 100)
                                {
                                    throw new FormatException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " desea buscar un valor VARCHAR de mas de 100 posiciones");
                                }
                            }
                        }
                    }
                    if (!ExisteTipoDato)
                    {
                        throw new FormatException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " desea buscar un tipo de atributo no reconocido");
                    }
                    if (TipodeDato != TipoColumnaEnArchivo)
                    {
                        throw new FormatException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " desea buscar un tipo de atributo que no coincide con la tabla");
                    }
                    if (TipodeDato != Datos.Instance.ListaAtributos.ElementAt(1))
                    {
                        if (InstruccionesSeparadas3[1].Trim().Split(' ').Length > 1)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee valores extras, debe de ser COLUMNA = DATO");
                        }
                    }
                    //-------------------------------select*from where id metodo2-------------
                    BTreeDLL.BTree<string, BTreeDLL.Tabla> CrearArbol = new BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreDeTabla + ".arbolb"), 8);
                    List<BTreeDLL.Tabla> ListaDatos = CrearArbol.goOverTreeInOrder();
                    List<BTreeDLL.Tabla> ListaParaTabla = new List<BTreeDLL.Tabla>();
                    for (int i = 0; i < ListaDatos.Count; i++)
                    {
                        for (int j = 0; j < ListaDatos.ElementAt(i).Objetos.Count; j++)
                        {
                            if (NombreColumnaBuscar == "ID")
                            {
                                if (ListaDatos.ElementAt(i).ID.ToString() == DatoABuscar.Trim())
                                {
                                    ListaParaTabla.Add(ListaDatos.ElementAt(i));
                                }
                            }
                            else
                            {
                                if (ListaDatos.ElementAt(i).Objetos.ElementAt(j).ToString() == DatoABuscar.Trim().Replace("'", ""))
                                {
                                    ListaParaTabla.Add(ListaDatos.ElementAt(i));
                                }
                            }
                        }
                    }
                    //aqui voy
                    TablaAMostrar Tabla = new TablaAMostrar();
                    Tabla.NombreColumnasArchivo = ColumnaEnArchivo;
                    Tabla.NombreColumnasAMostrar = ColumnaEnArchivo;
                    Tabla.ListaDatos = ListaParaTabla;
                    Tabla.DatosAMostrarSelect();
                    CrearArbol.CloseStream();
                }
                else
                {
                    //-------------------------------select*from metodo 3---------------
                    BTreeDLL.BTree<string, BTreeDLL.Tabla> ArbolCreadoArchivo = new BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreDeTabla + ".arbolb"), 8);
                    List<BTreeDLL.Tabla> ListaDatos = ArbolCreadoArchivo.goOverTreeInOrder();
                    FileStream ArchivoTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreDeTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    StreamReader ArchivoLector = new StreamReader(ArchivoTabla);
                    ColumnaEnArchivo = new List<string>();
                    TipoDatoEnArchivo = new List<string>();
                    string DatoActual;
                    ArchivoLector.ReadLine();
                    while ((DatoActual = ArchivoLector.ReadLine()) != null)
                    {
                        ColumnaEnArchivo.Add(DatoActual.Split(',')[0]);
                        TipoDatoEnArchivo.Add(DatoActual.Split(',')[1]);
                    }
                    TablaAMostrar Tabla = new TablaAMostrar();
                    Tabla.NombreColumnasArchivo = ColumnaEnArchivo;
                    Tabla.NombreColumnasAMostrar = ColumnaEnArchivo;
                    Tabla.ListaDatos = ListaDatos;
                    Tabla.DatosAMostrarSelect();
                    ArbolCreadoArchivo.CloseStream();
                }
            }
            else
            {
                //Dividimos por FROM
                string[] InstruccionesSeparadas = Regex.Split(Instucciones,Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
                if (InstruccionesSeparadas.Length==1)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key+ " no contiene "+ Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
                }
                if (InstruccionesSeparadas.Length>2)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " contiene " + Datos.Instance.diccionarioColeccionada.ElementAt(1).Key+" mas de una vez");
                }
                string[] ColumnasSoliccitadas = InstruccionesSeparadas[0].Trim().Split(',');
                if (InstruccionesSeparadas[1].Trim().Split(' ').Length>1)
                {

                }
            }
            //-------------------------------select columna from where metodo4-------------------
            //-------------------------------select columa from metodo5----------------------------------

        }
    }
    //--------------------------------DROP--------------------------------------------
    #region  Drop
    //-----------------------------Función de SQL que borra una tabla de MiniSQL-----------------------------
    public void DropTabla(string Valor)
    {
        Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key, "");//Quita la palabra reservada para la funciónn
        if (Valor.Trim().Split(' ').Length > 1)//Se comprueba que se tenga solo el nombre de la tabla que se eliminará
        {
            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(4).Key + " debe de poseer el nombre de la tabla que se eliminará");
        }
        string[] NombreTabla = Datos.Instance.ListaTablasExistentes.ToArray();//Existencia de la tabla que se desea eliminar
        bool ExistenciaTabla = false;
        for (int i = 0; i < NombreTabla.Length; i++)
        {
            if (NombreTabla[i] == Valor.Trim().Split(' ')[0].ToUpper())
            {
                ExistenciaTabla = true;
            }
<<<<<<< HEAD
=======
            //Elimina el archivo de Tabla & Arbol
            System.IO.File.Delete(@"~/microSQL/tablas/" + Valor.Trim().Split(' ')[0] + ".tabla");
            System.IO.File.Delete(Server.MapPath(@"~/microSQL/arbolesb/" + Valor.Trim().Split(' ')[0] + ".arbolb"));
>>>>>>> 950ecf6117d8d4f29fb91782252630ff7a559135
        }
        if (!ExistenciaTabla)
        {
            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(5).Key + " el nombre de la tabla no existe en el contexto actual");
        }
        //Elimina el archivo
    }
    #endregion

}





