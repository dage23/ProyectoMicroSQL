using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using ProyectoMicroSQL.Models;
using BTreeDLL;
using Tabla = BTreeDLL.Tabla;

namespace ProyectoMicroSQL.Controllers
{
    public class MicroSQLController : BaseController
    {
        #region Valores
        int SELECToPrimary_Key = 0;
        int FROMoVARCHAR = 1;
        int DELETEoINT = 2;
        int WHEREoDATETIME = 3;
        int CREATE_TABLE = 4;
        int DROP_TABLE = 5;
        int INSERT_INTO = 6;
        int VALUES = 7;
        int GO = 8;
        int UPDATE = 9;
        int SET = 10;
        #endregion

        #region Diccionarios
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
            Success(string.Format("Archivo importado exitosamente"), true);
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
            if (DiccionarioVar.FuncionSelect=="")
            {
                DiccionarioVar.FuncionSelect = "SELECT";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionSelect, "SELECT");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionSelect, "SELECT");
            }

            if (DiccionarioVar.FuncionFrom == "")
            {
                DiccionarioVar.FuncionFrom = "FROM";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionFrom, "FROM");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionFrom, "FROM");
            }

            if (DiccionarioVar.FuncionDelete == "")
            {
                DiccionarioVar.FuncionDelete = "DELETE";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDelete, "DELETE");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDelete, "DELETE");
            }

            if (DiccionarioVar.FuncionWhere == "")
            {
                DiccionarioVar.FuncionWhere = "WHERE";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionWhere, "WHERE");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionWhere, "WHERE");
            }

            if (DiccionarioVar.FuncionCreateTable == "")
            {
                DiccionarioVar.FuncionCreateTable = "CREATE TABLE";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionCreateTable, "CREATE TABLE");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionCreateTable, "CREATE TABLE");
            }

            if (DiccionarioVar.FuncionDropTable == "")
            {
                DiccionarioVar.FuncionDropTable = "DROP TABLE";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDropTable, "DROP TABLE");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionDropTable, "DROP TABLE");
            }

            if (DiccionarioVar.FuncionInsertInto == "")
            {
                DiccionarioVar.FuncionInsertInto = "INSERT INTO";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionInsertInto, "INSERT INTO");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionInsertInto, "INSERT INTO");
            }

            if (DiccionarioVar.FuncionValue == "")
            {
                DiccionarioVar.FuncionValue = "VALUES";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionValue, "VALUES");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionValue, "VALUES");
            }

            if (DiccionarioVar.FuncionGo == "")
            {
                DiccionarioVar.FuncionGo = "GO";
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionGo, "GO");
            }
            else
            {
                Datos.Instance.diccionarioColeccionada.Add(DiccionarioVar.FuncionGo, "GO");
            }
            Success(string.Format("Definiciones editadas exitosamente"), true);
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
        #endregion

        #region INSTRUCCIONES USUARIO
        //-------------------------------------------------ACCEDER A INSTRUCCIONES DE USUARIO------------------------------------
        [HttpPost]
        public ActionResult Data(FormCollection Sintaxis)
        {
            var OperacionSintaxis = new Sintaxis
            {
                SintaxisEscrita = Sintaxis["SintaxisEscrita"],
            };
            string DelimitadorGO = Datos.Instance.diccionarioColeccionada.ElementAt(GO).Key;
            string[] ArregloOperaciones = Regex.Split(OperacionSintaxis.SintaxisEscrita, DelimitadorGO);
            for (int i = 0; i < ArregloOperaciones.Length; i++)//Se guarda en una linea
            {
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], @"\r\n?|\n", "").Trim();//Se concatena
                ArregloOperaciones[i] = Regex.Replace(ArregloOperaciones[i], "  ", " ").Trim();//Se quitan las lineas
            }
            for (int i = 0; i < ArregloOperaciones.Length; i++)
            {
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key))
                {
                    //Crear Tabla   
                    CrearTablaYArbol(ArregloOperaciones[i]);
                }
                else if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key))
                {
                    //Insertar en Tabla   
                    InsertarEnTablaArbol(ArregloOperaciones[i]);
                }
                else if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key))
                {
                    //Seleccionar de Tabla   
                    SeleccionarDatosParaMostrar(ArregloOperaciones[i]);
                }
                else if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key))
                {
                    //Eliminar elemento
                    Eliminar(ArregloOperaciones[i]);
                }
                else if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(DROP_TABLE).Key))
                {
                    //Eliminar Archivo Tabla
                    DropTabla(ArregloOperaciones[i]);
                }
                else
                {
                    Danger("Nombre de instrucción no se encuentra en el diccionario", true);
                }
            }
            return View("DatosSQL");
        }
        #endregion

        #region CREATE
        private void CrearTablaYArbol(string Valor)
        {
            string NombreTabla = "";
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key, "").Trim();
            string[] SepararParentesis = Valor.Split(new char[] { '(' }, 2);
            //-----------------------------------------Errores de sintaxis----------------------
            NombreTabla = SepararParentesis[0].Trim();
            if (SepararParentesis.Length == 1)
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " no contiene parentesis de apertura"), true);
                return;
            }
            if (!Valor.Contains(Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key)))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " no contiene " + Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key)), true);
                return;
            }
            SepararParentesis[1] = SepararParentesis[1].Trim();
            if (!SepararParentesis[1][SepararParentesis[1].Length - 1].Equals(')'))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " no contiene parentesis de clausura"), true);
                return;
            }
            SepararParentesis[1] = SepararParentesis[1].Substring(0, SepararParentesis[1].Length - 1);
            string[] ValoresTabla = SepararParentesis[1].Split(',');
            if (ValoresTabla.Length < 2)
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " no puede insertar solo 1 atributo"), true);
                return;
            }
            if (ValoresTabla.Any(x => x.Trim() == ""))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " tiene error en las comas de separación de las columnas"), true);
                return;
            }
            for (int i = 0; i < ValoresTabla.Length; i++)
            {
                ValoresTabla[i] = ValoresTabla[i].Trim();
            }
            //-----------------------------Errores tipos de datos--------------------
            //Mas de 3 varchar
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR))).Count > 3)
            {
                Danger(string.Format("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR)), true);
                return;
            }
            //Mas de 3 int
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(DELETEoINT))).Count > 3)
            {
                Danger(string.Format("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(DELETEoINT)), true);
                return;
            }
            //Mas de 3 datetime
            if (Regex.Matches(Valor, Regex.Escape(Datos.Instance.ListaAtributos.ElementAt(WHEREoDATETIME))).Count > 3)
            {
                Danger(string.Format("Contiene mas de tres " + Datos.Instance.ListaAtributos.ElementAt(WHEREoDATETIME)), true);
                return;
            }
            //--------------------------------Operaciones con Primary key---------------------------------
            int TamanoKey = Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key).Trim().Split(' ').Length;
            //--------------------------------Errores--------------------------------------------------
            if (ValoresTabla[0].Split(' ').Length != 2 + TamanoKey)
            {
                Danger(string.Format(Datos.Instance.ListaAtributos.ElementAt(CREATE_TABLE) + " debe de contener 3 campos[NOMBRE TIPO LLAVE]"), true);
                return;
            }
            if (ValoresTabla[0].Split(' ')[0].ToUpper() != "ID")
            {
                Danger(string.Format(Datos.Instance.ListaAtributos.ElementAt(CREATE_TABLE) + " debe contener ID como primer campo"), true);
                return;
            }
            if (ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(DELETEoINT) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(WHEREoDATETIME))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " utiliza un atributo no permitido, debe de ser tipo " + Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key)), true);
                return;
            }
            if (ValoresTabla[0].Split(' ')[1] != Datos.Instance.ListaAtributos.ElementAt(DELETEoINT))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + ", el primer dato debe ser un tipo " + Datos.Instance.ListaAtributos.ElementAt(DELETEoINT)), true);
                return;
            }
            //--------------------------------------TIpo de llave primaria-------------------------------
            string LlavePrimaria = " ";
            for (int i = 2; i < ValoresTabla[0].Split(' ').Length; i++)
            {
                LlavePrimaria += ValoresTabla[0].Split(' ')[i] + " ";
            }
            LlavePrimaria = LlavePrimaria.Trim();
            if (LlavePrimaria != Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key))
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + ", el primer dato debe ser un " + Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key)), true);
                return;
            }
            //----------------------------------------Creacion de archivos------------------------------
            var ListaNombreColumna = new List<string>();
            var ListaTipoColumnas = new List<string>();
            var ArregloNombreColumnas = new string[9];
            ArregloNombreColumnas[0] = "ID";
            ListaNombreColumna.Add("ID");
            ListaTipoColumnas.Add("INT");
            for (int i = 1; i < ValoresTabla.Length; i++)
            {
                //--------------------------------------Errores de atributos--------------------
                if (ValoresTabla[i].Split(' ').Length != 2)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " los atributos que no son PRIMARY KEY no deben de tener mas de dos campos"), true);
                    return;
                }
                if (ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(SELECToPrimary_Key) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(DELETEoINT) ||
                ValoresTabla[0].Split(' ')[0] == Datos.Instance.ListaAtributos.ElementAt(WHEREoDATETIME))
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + " los atributos que no son PRIMARY KEY deben contener el nombre en el campo 1"), true);
                    return;
                }
                if (ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(DELETEoINT) &&
                ValoresTabla[i].Split(' ')[1] == Datos.Instance.ListaAtributos.ElementAt(WHEREoDATETIME))
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + ", los atributos que no son PRIMARY KEY deben contener el tipo en el campo 2"), true);
                    return;
                }
                if (ArregloNombreColumnas.Any(x => ValoresTabla[i].Split(' ')[0].ToUpper() == x))
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + ", no se pueden repetir nombres de columnas"), true);
                    return;
                }
                ArregloNombreColumnas[i] = ValoresTabla[i].Split(' ')[0].ToUpper();
                ListaNombreColumna.Add(ValoresTabla[i].Split(' ')[0].ToUpper());
                ListaTipoColumnas.Add(ValoresTabla[i].Split(' ')[1].ToUpper());
            }
            string[] ArregloListaNombreTablas = Datos.Instance.ListaTablasExistentes.ToArray();
            //if (Datos.Instance.ListaTablasExistentes.Count() > 0)
            //{
            //    for (int i = 0; i < ArregloListaNombreTablas.Length; i++)
            //    {
            //        if (ArregloListaNombreTablas[i] == NombreTabla.ToUpper())
            //        {
            //            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(CREATE_TABLE).Key + ", el nombre " + NombreTabla + " no puede repetirse"), true);
            //            return;
            //        }
            //    }
            //}
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
                    ValoresdeTabla += ListaNombreColumna[i];
                }
                else
                {
                    ValoresdeTabla += ListaNombreColumna[i] + ", ";
                }
            }
            string TipoValoresTabla = "";
            for (int i = 0; i < ListaTipoColumnas.Count; i++)
            {
                if (i == ListaTipoColumnas.Count - 1)
                {
                    TipoValoresTabla += ListaTipoColumnas[i];
                }
                else
                {
                    TipoValoresTabla += ListaTipoColumnas[i] + ", ";
                }
            }
            Datos.Instance.ListaTablaYValores.Add(new Listado_Tablas { NombreTabla = NombreTabla, ValoresTabla = ValoresdeTabla, TipoValoresTabla = TipoValoresTabla });
            Success(string.Format("Tabla creada exitosamente"), true);
        }
        //-------------------------------Crear archivo.tabla-----------
        private void CrearArchivoTabla(List<string> NombreColumnas, List<string> TipoColumnas, string Nombre)
        {
            var ArchivoTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + Nombre + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var Escritor = new StreamWriter(ArchivoTabla);
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
            var CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreArbol + ".arbolb"), 5);
            CrearArbol.CloseStream();
        }
        #endregion

        #region INSERT
        //-------------------------------INSERT INTO----------------------------------------
        private void InsertarEnTablaArbol(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key, "").Trim();
            if (Valor.Count(x => x == '(') != 2 || Valor.Count(y => y == ')') != 2)
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " necesitas revisar la sintaxis de tu codigo (PARENTESIS)"), true);
                return;
            }
            string[] SeparadorComas = Valor.Split(new char[] { '(' }, 2);
            SeparadorComas[0] = SeparadorComas[0].Trim();
            if (SeparadorComas[0].Split(' ').Length > 1)
            {
                Danger(string.Format("El nombre de la tabla no puede ser mayor de dos campos"), true);
                return;
            }
            string NombreTablaInsertar = SeparadorComas[0];
            //bool TablaEncontradaEnLista = false;
            //for (int i = 0; i < Datos.Instance.ListaTablasExistentes.Count(); i++)
            //{
            //    if ((NombreTablaInsertar.ToUpper() == Datos.Instance.ListaTablasExistentes[i]))
            //    {
            //        TablaEncontradaEnLista = true;
            //        break;
            //    }
            //}
            //if (!TablaEncontradaEnLista)
            //{
            //    Danger(string.Format("El nombre " + NombreTablaInsertar + " no existe en ninguna tabla"), true);
            //    return;
            //}
            var TablaEnArhivo = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTablaInsertar + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var Lector = new StreamReader(TablaEnArhivo);
            var ColumnaEnArchivo = new List<string>();
            var TipoEnArchivo = new List<string>();
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
                Danger(string.Format("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + ", el numero de atributos de atributos no concuerda con los de la tabla"), true);
                return;
            }

            string[] ColumnaInstruccion = new string[ColumnaEnArchivo.Count];
            ColumnaInstruccion = SepararPorPrimerParentesis[0].Trim().Split(',');

            for (int i = 0; i < ColumnaInstruccion.Length; i++)
            {
                if (ColumnaInstruccion[i].Trim().ToUpper() != ColumnaEnArchivo.ElementAt(i))
                {
                    Danger(string.Format("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + ", no coinciden los atributos de la tabla con los almacenados"), true);
                    return;
                }
            }

            string[] SepararPorSegundoParentesis = SepararPorPrimerParentesis[1].Trim().Split(new char[] { '(' }, 2);
            if (SepararPorSegundoParentesis[0].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(VALUES).Key)
            {
                Danger(string.Format("La instruccion " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + "no posee" + Datos.Instance.diccionarioColeccionada.ElementAt(VALUES).Key), true);
                return;
            }

            string[] SepararPorTercerParentesis = SepararPorSegundoParentesis[1].Trim().Split(new char[] { ')' }, 2);
            if (SepararPorTercerParentesis[1].Trim() != "")
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " tiene un error en los parentesis"), true);
                return;
            }

            if (SepararPorTercerParentesis[0].Trim().Split(',').Length != TipoEnArchivo.Count)
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " no introduce la cantidad de valores requeridos"), true);
                return;
            }


            var ListaObjetosAInsertar = new List<object>();
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
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " no permite ingresar datos nulos"), true);
                            return;
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
                                Danger(string.Format("En " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " no puedes insertar VARCHAR mayores a 100"), true);
                                return;
                            }
                            ListaObjetosAInsertar.Add(ArregloDAtosInsertar[i]);
                        }
                    }
                }
                if (!TipoDatoExiste)
                {
                    Danger(string.Format("En " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + " no se reconoce un tipo de dato a insertar"), true);
                    return;
                }
                if (TipoDatoInsertar != TipoEnArchivo.ElementAt(i))
                {
                    Danger(string.Format("En " + Datos.Instance.diccionarioColeccionada.ElementAt(INSERT_INTO).Key + ", hay un campo que no concuerda con la tabla"), true);
                    return;
                }
            }
            //Si todo esta bien
            var TablaAInsertarEnArbol = new BTreeDLL.Tabla(IDInsertar, ListaObjetosAInsertar);
            var ArbolACrear = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTablaInsertar + ".arbolb"), 5);

            ArbolACrear.AddElement(TablaAInsertarEnArbol);
            ArbolACrear.CloseStream();
            Success(string.Format("Insercion exitosa"), true);
        }
        #endregion

        #region SELECT
        //-------------------------------SELECT-------------------------------------------
        private void SeleccionarDatosParaMostrar(string Instucciones)
        {
            if (!Instucciones.Contains(Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key))
            {
                Danger(string.Format("El metodo " + Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " no contiene" + Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key), true);
                return;
            }
            Instucciones = Instucciones.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key, "").Trim();
            List<string> ColumnaEnArchivo;
            List<string> TipoDatoEnArchivo;
            if (Instucciones.Trim().Split(' ')[0] == "*")
            {
                //--------------------------------select*from metodo1---------------
                if (Instucciones.Trim().Split(' ')[1] != Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key)
                {
                    Danger(string.Format("Posee un error de sintaxis en el metodo " + Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key), true);
                    return;
                }
                string[] InstuccionesSeparadas = Regex.Split(Instucciones, Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key);
                bool ExisteTabla = false;
                string NombreDeTabla = InstuccionesSeparadas[1].Trim().Split(' ')[0];
                //for (int i = 0; i < Datos.Instance.ListaTablasExistentes.Count; i++)
                //{
                //    if (Datos.Instance.ListaTablasExistentes[i] == NombreDeTabla.ToUpper())
                //    {
                //        ExisteTabla = true;
                //        break;
                //    }
                //}
                //if (!ExisteTabla)
                //{
                //    Danger(string.Format("En el comando " + Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " declara una tabla inexistente"), true);
                //    return;
                //}
                //Posee escrito where
                if (InstuccionesSeparadas[1].Trim().Split(' ').Length > 1)
                {
                    //---------------------Errores sintaxis-------------------------
                    if (InstuccionesSeparadas[1].Trim().Split(' ')[1] != Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key)
                    {
                        Danger(string.Format("Posee un error de escritura con la funcion " + Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key), true);
                        return;
                    }
                    string[] InstruccionesSeparadas2 = Regex.Split(InstuccionesSeparadas[1].Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key);
                    InstruccionesSeparadas2[1] = InstruccionesSeparadas2[1].Trim();

                    string[] InstruccionesSeparadas3 = Regex.Split(InstruccionesSeparadas2[1].Trim(), "=");
                    if (InstruccionesSeparadas3.Length != 2)
                    {
                        Danger(string.Format("El comando " + Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee error de escritura en condicion"), true);
                        return;
                    }

                    if (InstruccionesSeparadas3[1].Trim() == "")
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee un error en la condicion"), true);
                        return;
                    }

                    if (InstruccionesSeparadas3[0].Trim() == "")
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee un error en la condicion"), true);
                        return;
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
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " busca un dato null"), true);
                            return;
                        }
                        if (InstruccionesSeparadas3[1].Trim().Length != InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1).Length)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee valores extras, debe de ser COLUMNA = DATO"), true); return;
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
                    var ArchivoDeTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreDeTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    var LectorTabla = new StreamReader(ArchivoDeTabla);
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
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " utiliza el nombre de columna inexistente"), true); return;
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
                                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " desea buscar un valor VARCHAR de mas de 100 posiciones"), true); return;
                                }
                            }
                        }
                    }
                    if (!ExisteTipoDato)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " desea buscar un tipo de atributo no reconocido"), true); return;
                    }
                    if (TipodeDato != TipoColumnaEnArchivo)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " desea buscar un tipo de atributo que no coincide con la tabla"), true); return;
                    }
                    if (TipodeDato != Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR))
                    {
                        if (InstruccionesSeparadas3[1].Trim().Split(' ').Length > 1)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee valores extras, debe de ser COLUMNA = DATO"), true); return;
                        }
                    }
                    //-------------------------------select*from where id metodo2-------------
                    var CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreDeTabla + ".arbolb"), 5);
                    List<BTreeDLL.Tabla> ListaDatos = CrearArbol.goOverTreeInOrder();
                    var ListaParaTabla = new List<BTreeDLL.Tabla>();
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
                    var Tabla = new TablaAMostrar();
                    Tabla.NombreColumnasArchivo = ColumnaEnArchivo;
                    Tabla.NombreColumnasAMostrar = ColumnaEnArchivo;
                    Tabla.ListaDatos = ListaParaTabla;
                    Tabla.DatosAMostrarSelect();
                    CrearArbol.CloseStream();
                }
                else
                {
                    //-------------------------------select*from metodo 3---------------
                    var ArbolCreadoArchivo = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreDeTabla + ".arbolb"), 5);
                    List<BTreeDLL.Tabla> ListaDatos = ArbolCreadoArchivo.goOverTreeInOrder();
                    var ArchivoTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreDeTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    var ArchivoLector = new StreamReader(ArchivoTabla);
                    ColumnaEnArchivo = new List<string>();
                    TipoDatoEnArchivo = new List<string>();
                    string DatoActual;
                    ArchivoLector.ReadLine();
                    while ((DatoActual = ArchivoLector.ReadLine()) != null)
                    {
                        ColumnaEnArchivo.Add(DatoActual.Split('|')[0]);
                        TipoDatoEnArchivo.Add(DatoActual.Split('|')[1]);
                    }
                    var Tabla = new TablaAMostrar();
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
                string[] InstruccionesSeparadas = Regex.Split(Instucciones, Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key);
                if (InstruccionesSeparadas.Length == 1)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " no contiene " + Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key), true); return;
                }
                if (InstruccionesSeparadas.Length > 2)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " contiene " + Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key + " mas de una vez"), true); return;
                }
                string[] ColumnasSolicitadas = InstruccionesSeparadas[0].Trim().Split(',');
                if (InstruccionesSeparadas[1].Trim().Split(' ').Length > 1)
                {
                    bool UlilizaWhere = false;
                    string NombreTabla = InstruccionesSeparadas[1].Trim().Split(' ')[0];

                    if (InstruccionesSeparadas[1].Trim().Split(' ')[1].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " debe de tener " + Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key + " despues"), true); return;
                    }
                    string[] InstruccionesSeparadas2 = Regex.Split(InstruccionesSeparadas[1].Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key);
                    InstruccionesSeparadas2[1] = InstruccionesSeparadas2[1].Trim();
                    string[] InstruccionesSeparadas3 = Regex.Split(InstruccionesSeparadas2[1].Trim(), "=");
                    if (InstruccionesSeparadas3.Length != 2)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee un error de condicion."), true); return;
                    }
                    if (InstruccionesSeparadas3[1].Trim() == "")
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee error despues de ="), true); return;
                    }
                    if (InstruccionesSeparadas3[0].Trim() == "")
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee un error antes de ="), true); return;
                    }
                    string NombreColumna = InstruccionesSeparadas3[0].Trim();
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
                                    ConteoFinal = i; break;
                                }
                            }
                        }
                        if (ConteoFinal < 2)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " trata de buscar un valor nulo"), true); return;
                        }
                        if (InstruccionesSeparadas3[1].Trim().Length != InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1).Length)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee valores adicionales no permitidos"), true); return;
                        }
                        InstruccionesSeparadas3[1] = InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1);
                    }
                    string dato = "";
                    if (EsVarChar)
                    {
                        dato = InstruccionesSeparadas3[1].Trim();
                    }
                    else
                    {
                        dato = InstruccionesSeparadas3[1].Trim().Split(' ')[0];
                    }
                    string tipoDato = "";
                    var ArchivoDeTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Datos.Instance.NombreTabla = NombreTabla;
                    var LectorTabla = new StreamReader(ArchivoDeTabla);
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
                    for (int i = 0; i < ColumnaEnArchivo.Count; i++)
                    {
                        if (ColumnaEnArchivo.ElementAt(i) == NombreColumna.ToUpper())
                        {
                            TipoColumnaEnArchivo = TipoDatoEnArchivo.ElementAt(i);
                            ExisteColumna = true;
                        }
                    }
                    if (!ExisteColumna)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + ", la columnaa buscar no existe"), true); return;
                    }

                    bool ExisteTipoDato = false;
                    tipoDato = "";
                    try
                    {
                        //Es int
                        int PruebaConversionInt = int.Parse(dato);
                        tipoDato = "INT";
                        ExisteTipoDato = true;
                    }
                    catch (FormatException)
                    {
                        //No es int
                    }
                    //Datetime o varchar
                    if (dato.Length > 2 && !ExisteTipoDato)
                    {
                        if (dato[0] == '\'' && dato[dato.Length - 1] == '\'')
                        {
                            dato = dato.Replace("'", "");
                            try
                            {
                                //Es datetime
                                DateTime PruebaConversionDatetime = DateTime.Parse(dato);
                                tipoDato = "DATETIME";
                                ExisteTipoDato = true;
                            }
                            catch (FormatException)
                            {
                                //Es varchar
                                tipoDato = "VARCHAR(100)";
                                ExisteTipoDato = true;
                                if (dato.Length > 100)
                                {
                                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " desea buscar un valor VARCHAR de mas de 100 posiciones"), true); return;
                                }
                            }
                        }
                    }
                    if (!ExisteTipoDato)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + ", desea  buscar un tipo de dato no reconocido"), true); return;
                    }

                    if (tipoDato != TipoColumnaEnArchivo)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + ", desea  buscar un tipo de dato que no concuerda con la tabla"), true); return;
                    }

                    if (tipoDato != Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR))
                    {
                        if (InstruccionesSeparadas3[1].Trim().Split(' ').Length > 1)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + ", posee un error de escritura, por favor revisar [columna=dato]"), true); return;
                        }
                    }
                    string[] ArregloColumnaRepetidas = new string[ColumnasSolicitadas.Length];
                    for (int i = 0; i < ArregloColumnaRepetidas.Length; i++)
                    {
                        ArregloColumnaRepetidas[i] = "";
                    }
                    //----------------------Comprobar que existan columnas-------------------------------------
                    bool ColumnaExisten = false;
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnaExisten = false;
                        for (int j = 0; j < ArregloColumnaRepetidas.Length; j++)
                        {
                            if (ColumnasSolicitadas[i].Trim().ToUpper() == ArregloColumnaRepetidas[j].Trim())
                            {
                                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee columnas repetidas"), true); return;
                            }
                        }
                        for (int k = 0; k < ColumnaEnArchivo.Count; k++)
                        {
                            if (ColumnasSolicitadas[i].Trim().ToUpper() == ColumnaEnArchivo.ElementAt(k))
                            {
                                ColumnaExisten = true;
                                break;
                            }
                        }
                        if (!ColumnaExisten)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee una columna mal colocada, por favor revisa el orden de insercion de codigo"), true); return;
                        }
                        ArregloColumnaRepetidas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    //-------------------------------select columna from where metodo4-------------------
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnasSolicitadas[i] = ColumnasSolicitadas[i].Trim();
                    }
                    var CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTabla + ".arbolb"), 5);
                    Datos.Instance.NombreTabla = NombreTabla;
                    List<BTreeDLL.Tabla> Listadatos = CrearArbol.goOverTreeInOrder();
                    var DatosTabla = new List<BTreeDLL.Tabla>();
                    //Datos a para tabla
                    for (int i = 0; i < Listadatos.Count; i++)
                    {
                        for (int j = 0; j < Listadatos.ElementAt(i).Objetos.Count; j++)
                        {
                            if (NombreColumna == "ID")
                            {
                                if (Listadatos.ElementAt(i).ID.ToString() == dato.Trim())
                                {
                                    DatosTabla.Add(Listadatos.ElementAt(i));
                                }
                            }
                            else
                            {
                                if (Listadatos.ElementAt(i).Objetos.ElementAt(j).ToString() == dato.Trim().Replace("'", ""))
                                {
                                    DatosTabla.Add(Listadatos.ElementAt(i));
                                }
                            }
                        }
                    }
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnasSolicitadas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    var tablaAMostrar = new TablaAMostrar();
                    tablaAMostrar.NombreColumnasArchivo = ColumnaEnArchivo;
                    tablaAMostrar.NombreColumnasAMostrar = ColumnasSolicitadas.ToList();
                    tablaAMostrar.ListaDatos = DatosTabla;
                    tablaAMostrar.DatosAMostrarSelect();
                    CrearArbol.CloseStream();
                }
                //-------------------------------select columa from metodo5----------------------------------
                else
                {
                    bool ExisteTabla = false;
                    string nombreTabla = InstruccionesSeparadas[1].Trim().Split(' ')[0];
                    string[] tablas = Datos.Instance.ListaTablasExistentes.ToArray();
                    //for (int i = 0; i < tablas.Length; i++)
                    //{
                    //    if (tablas[i] == nombreTabla.ToUpper())
                    //    {
                    //        ExisteTabla = true;
                    //    }
                    //}
                    //if (!ExisteTabla)
                    //{
                    //    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + ", la tabla no existe."), true); return;
                    //}
                    var tabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + nombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Datos.Instance.NombreTabla = nombreTabla;
                    var Lector = new StreamReader(tabla);
                    ColumnaEnArchivo = new List<string>();
                    TipoDatoEnArchivo = new List<string>();
                    string[] ArregloColumnasRepetidas = new string[ColumnasSolicitadas.Length];
                    string TipoActual;
                    Lector.ReadLine();
                    for (int i = 0; i < ArregloColumnasRepetidas.Length; i++)
                    {
                        ArregloColumnasRepetidas[i] = "";
                    }
                    while ((TipoActual = Lector.ReadLine()) != null)
                    {
                        ColumnaEnArchivo.Add(TipoActual.Split('|')[0]);
                        TipoDatoEnArchivo.Add(TipoActual.Split('|')[1]);
                    }
                    tabla.Close();
                    bool ExisteTodasColumnas = false;
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ExisteTodasColumnas = false;
                        for (int j = 0; j < ArregloColumnasRepetidas.Length; j++)
                        {
                            if (ColumnasSolicitadas[i].Trim().ToUpper() == ArregloColumnasRepetidas[j].Trim())
                            {
                                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee una columna repetida"), true); return;
                            }
                        }
                        for (int k = 0; k < ColumnaEnArchivo.Count; k++)
                        {
                            if (ColumnasSolicitadas[i].Trim().ToUpper() == ColumnaEnArchivo.ElementAt(k))
                            {
                                ExisteTodasColumnas = true; break;
                            }
                        }
                        if (!ExisteTodasColumnas)
                        {
                            Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(SELECToPrimary_Key).Key + " posee una columna que no esta en la posicion correcta"), true); return;
                        }
                        ArregloColumnasRepetidas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnasSolicitadas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    var CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + nombreTabla + ".arbolb"), 5);
                    Datos.Instance.NombreTabla = nombreTabla;
                    List<BTreeDLL.Tabla> datosTablas = CrearArbol.goOverTreeInOrder();
                    var tablaAMostrar = new TablaAMostrar();
                    tablaAMostrar.NombreColumnasArchivo = ColumnaEnArchivo;
                    tablaAMostrar.NombreColumnasAMostrar = ColumnasSolicitadas.ToList();
                    tablaAMostrar.ListaDatos = datosTablas;
                    tablaAMostrar.DatosAMostrarSelect();
                    CrearArbol.CloseStream();
                }
            }
            ConvertirEnLista();
            RedirectToAction("VerTablaSelect");
            Success(string.Format("Operacion select exitosa, para revisar su seleccion dirijase a -Tabla Seleccionada-"), true);
        }
        #endregion

        #region  DROP
        //-----------------------------Función de SQL que borra una tabla de MiniSQL-----------------------------
        public void DropTabla(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(DROP_TABLE).Key, "");//Quita la palabra reservada para la funciónn
            if (Valor.Trim().Split(' ').Length > 1)//Se comprueba que se tenga solo el nombre de la tabla que se eliminará
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DROP_TABLE).Key + " debe de poseer el nombre de la tabla que se eliminará"), true); return;
            }
            string[] NombreTabla = Datos.Instance.ListaTablasExistentes.ToArray();//Existencia de la tabla que se desea eliminar
            bool ExistenciaTabla = false;
            for (int i = 0; i < NombreTabla.Length; i++)
            {
                if (NombreTabla[i] == Valor.Trim().Split(' ')[0].ToUpper())
                {
                    ExistenciaTabla = true;
                }
            }
            if (!ExistenciaTabla)
            {
                Danger(string.Format("El nombre de la tabla a eliminar no existe"), true); return;
            }
            //Elimina el archivo de Lista, Tabla & Árbol
            for (int i = 0; i < Datos.Instance.ListaTablasExistentes.Count; i++)
            {
                if (Datos.Instance.ListaTablasExistentes.ElementAt(i) == Valor.Trim().Split(' ')[0].ToUpper())
                {
                    Datos.Instance.ListaTablasExistentes.Remove(NombreTabla[i]);
                }
                if (Datos.Instance.ListaTablaYValores.ElementAt(i).NombreTabla == Valor.Trim().Split(' ')[0].ToUpper())
                {
                    Datos.Instance.ListaTablaYValores.RemoveAt(i);
                }
            }
            System.IO.File.Delete(Server.MapPath(@"~/microSQL/tablas/" + Valor.Trim().Split(' ')[0] + ".tabla"));
            System.IO.File.Delete(Server.MapPath(@"~/microSQL/arbolesb/" + Valor.Trim().Split(' ')[0] + ".arbolb"));
            Success(string.Format("Tabla eliminada exitosamente"), true);
        }
        #endregion

        #region ELIMINAR
        public void Eliminar(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key, "").Trim();//Eliminar la palabra reservada para la acción
            if (Valor.Split(' ').Length < 2)//Comprueba que tenga almenos 2 campos
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " está incompleto"), true); return;
            }
            if (Valor.Split(' ')[0].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(FROMoVARCHAR).Key)//Sintaxis erronea o no completa
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " sintaxis incorrecta o incompleta"), true); return;
            }
            string NombreTabla = Valor.Split(' ')[1].Trim();
            if (Valor.Split(' ').Length > 2)//Se comprueba si contiene WHERE
            {
                if (Valor.Split(' ')[2] != Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key)//Cuando tenga
                {
                    Danger(string.Format("Despues de " + Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " debe de ir " + Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key), true); return;
                }
                string[] DivValor2 = Regex.Split(Valor.Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key);
                DivValor2[1] = DivValor2[1].Trim();
                string[] DivValor3 = Regex.Split(DivValor2[1].Trim(), "=");//ID=1
                if (DivValor3.Length != 2)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " error en la condición"), true); return;
                }
                if (DivValor3[0].Trim() == "")
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " no hay nada del lado izquierdo a la igualación"), true); return;
                }
                if (DivValor3[1].Trim() == "")
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " no hay nada del lado derecho a la igualación"), true); return;
                }
                string Columna = DivValor3[0].Trim();
                int Final = 0;
                int Conteo = 0;
                bool VarChar = false;
                if (DivValor3[1].Trim()[0] == '\'')
                {
                    VarChar = true;
                    for (int i = 0; i < DivValor3[1].Trim().Length; i++)
                    {
                        Conteo++;
                        if (Conteo > 1)
                        {
                            Final = i;
                            break;
                        }
                    }
                    if (Final < 2)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " buscador en WHERE es nulo"), true); return;
                    }
                    if (DivValor3[1].Trim().Length != DivValor3[1].Trim().Substring(0, Final + 1).Length)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + ", por favor escribir solamente [Columna = Dato]"), true); return;
                    }
                    DivValor3[1] = DivValor3[1].Trim().Substring(0, Final + 1);

                }
            //---------------------------Posibles errores de sintaxis---------------------------                     
            
                string DatoVarchar = "";
                if (VarChar)
                {
                    DatoVarchar = DivValor3[1].Trim();
                }
                else
                {
                    DatoVarchar = DivValor3[1].Trim().Split(' ')[0];
                }

                //Comprobar que exista la columna
                var GTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var Lectura = new StreamReader(GTabla);
                var ColumnaArchivo = new List<string>();
                var Formato = new List<string>();
                string Tipo;
                Lectura.ReadLine();
                while ((Tipo = Lectura.ReadLine()) != null)
                {
                    ColumnaArchivo.Add(Tipo.Split('|')[0]);
                    Formato.Add(Tipo.Split('|')[1]);
                }
                GTabla.Close();

                VarChar = false;
                var TipoColumArchivo = "";
                var ColumPos = -1;

                for (int i = 0; i < ColumnaArchivo.Count(); i++)
                {
                    if (ColumnaArchivo.ElementAt(i) == Columna)
                    {
                        TipoColumArchivo = Formato.ElementAt(i);
                        ColumPos = i - 1;
                        VarChar = true;
                    }
                }
                if (!VarChar)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " la columna no existe"), true); return;
                }
                VarChar = false;

                var TipoDato = saberTipo(DatoVarchar);
                if (TipoDato != TipoColumArchivo)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " los campos en la condición no tienen el mismo tipo que la columna"), true); return;
                }
                if (TipoDato != Datos.Instance.ListaAtributos.ElementAt(FROMoVARCHAR))
                {
                    if (DivValor3[1].Trim().Split(' ').Length > 1)
                    {
                        Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + ", por favor escribir [Columna = Dato]"), true); return;
                    }
                }
                VarChar = false;

                var ArbolACrear = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTabla + ".arbolb"), 5);
                List<BTreeDLL.Tabla> DatosLista = ArbolACrear.goOverTreeInOrder();
                var TablaEliminar = new BTreeDLL.Tabla(int.Parse(DatoVarchar), null);
                if (ArbolACrear.SearchElementTree(TablaEliminar) == null)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " el ID no existe en el arbol"), true); return;
                }
                if (Columna == "ID")//Eliminar elemento por ID
                {
                    var DatoEliminar = new BTreeDLL.Tabla(int.Parse(DatoVarchar), null);
                    ArbolACrear.DeleteElement(DatoEliminar);
                }
                else
                {
                    for (int i = 0; i < DatosLista.Count; i++)
                    {
                        for (int j = 0; j < DatosLista.ElementAt(i).Objetos.Count; j++)
                        {
                            switch (saberTipo(DatoVarchar))
                            {
                                case "VARCHAR(100)":
                                    DatoVarchar = DatoVarchar.Replace("'", "");
                                    if (j == ColumPos && DatosLista.ElementAt(i).Objetos.ElementAt(j).ToString().Replace("#", "") == DatoVarchar)
                                    {
                                        var Eliminar_VARCHAR = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
                                        ArbolACrear.DeleteElement(Eliminar_VARCHAR);
                                    }
                                    break;
                                case "INT":
                                    if (j == ColumPos && Convert.ToInt32(DatosLista.ElementAt(i).Objetos.ElementAt(j)) == int.Parse(DatoVarchar))
                                    {
                                        var Eliminar_INT = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
                                        ArbolACrear.DeleteElement(Eliminar_INT);
                                    }
                                    break;
                                case "DATETIME":
                                    var Eliminar_DATETIME = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
                                    ArbolACrear.DeleteElement(Eliminar_DATETIME);
                                    break;
                            }
                        }
                    }
                }
                ArbolACrear.CloseStream();
            }
            else//Se eliminan todos los datos, no tiene WHERE
            {
                System.IO.File.Delete(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTabla + ".arbolb"));
                var ArbolCrear = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".arbolb"), 5);
                ArbolCrear.CloseStream();
            }
            Success(string.Format("Eliminar exitoso"), true);
        }
        #endregion

        #region UPDATE
        public void Update(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key, "").Trim();
            var NombreTablaActu = Valor.Split(' ')[0].Trim();
            string[] Tabla = Datos.Instance.ListaTablasExistentes.ToArray();
            bool EsTabla = false;
            for (int i = 0; i < NombreTablaActu.Length; i++)//Se recorre para comprobar si tabla existe
            {
                if (Tabla[i] == NombreTablaActu.ToUpper())
                {
                    EsTabla = true;
                }
            }
            if (!EsTabla)//Tabla no existe
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " el nombre de la tabla no existe"), true); return;
            }
            string[] ValorDiv = Regex.Split(Valor.Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(SET).Key);
            if (ValorDiv.Length == 1)//No contiene SET en sintaxis
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " no contiene SET en la sintaxis"), true); return;
            }
            if (ValorDiv[0].Trim().Split(' ').Length > 1)//Comprueba que el campo sea UPDATE en el tercer campo
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " debe de ser el tercer campo"), true); return;
            }
            string[] ValorDiv2 = Regex.Split(Valor.Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(WHEREoDATETIME).Key);//Se va a comprobar que contenga 'WHERE'
            if (ValorDiv2.Length == 1)
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " no encuentra 'WHERE' en la sintaxis"), true); return;
            }

            string[] ValordeColumna = Regex.Split(ValorDiv2[0], "");//Se utiliza para cambiar el ValorDiv2 y comprobar que ID = 1
            if (ValordeColumna.Length != 2)//Contiene mas de 2 hay mas de un igual
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " contiene error en el SET"), true); return;
            }
            if (ValordeColumna[0].Trim() == "")//La posición 0 está vacia... No hay nada del lado izquierdo
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " no contiene nada del lado izquierdo del igual del SET"), true); return;
            }
            if (ValordeColumna[1].Trim() == "")//La posición 0 está vacia... No hay nada del lado derecho
            {
                Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " no contiene nada del lado derecho del igual del SET"), true); return;
            }

            int contFin = 0;
            int cont = 0;
            bool varchar = false;//Se utiliza para tomar ciertas restricciones
            if (ValordeColumna[1].Trim()[0] == '\'')//Se comprueba el ultimo LENGHT
            {

                varchar = true;
                for (int i = 0; i < ValordeColumna[1].Trim().Length; i++)
                {
                    if (ValordeColumna[1].Trim()[i] == '\'')
                    {
                        cont++;
                        if (cont > 1)
                        {
                            contFin = i;
                            break;
                        }
                    }
                }
                if (contFin < 2)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " el valor en WHERE es nulo"), true); return;
                }
                if (ValordeColumna[1].Trim().Length != ValordeColumna[1].Trim().Substring(0, contFin + 1).Length)
                {
                    Danger(string.Format(Datos.Instance.diccionarioColeccionada.ElementAt(UPDATE).Key + " escriba [Columna = Dato] sin nada a la derecha o hacia abajo"), true); return;
                }
                ValordeColumna[1] = ValordeColumna[1].Trim().Substring(0, contFin + 1);
            }
            var Info = "";
            if (varchar)
            {
                Info = ValordeColumna[1].Trim();
            }
            else
            {
                Info = ValordeColumna[1].Trim().Split(' ')[0];
            }

            var DatoTipo = "";
            //comprobar que exista la columna & obtener numero de parametros
            var GTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTablaActu + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var Lectura = new StreamReader(GTabla);
            var ColumnaArchivo = new List<string>();
            var Formato = new List<string>();
            string Actual;
            Lectura.ReadLine();
            while ((Actual = Lectura.ReadLine()) != null)//Se llena las columnas y tipos
            {
                ColumnaArchivo.Add(Actual.Split(',')[0]);
                Formato.Add(Actual.Split(',')[1]);
            }
            GTabla.Close();
        }
        #endregion

        #region Utilidad
        public string saberTipo(string Valor)
        {
            string tipoDato = "";
            try
            {
                //Es tipo INT
                int pruebaParseInt = int.Parse(Valor);
                tipoDato = "INT";
            }
            catch (FormatException)
            {
                //No es tipo int
            }

            //Si tiene lenght mayor a dos se puede comprobar si es una fecha o varchar
            if (Valor.Length > 2)
            {
                if (Valor[0] == '\'' && Valor[Valor.Length - 1] == '\'')
                {
                    Valor = Valor.Replace("'", "");

                    try
                    {
                        //Es Datetime
                        DateTime pruebaParseDate = DateTime.Parse(Valor);
                        tipoDato = "DATETIME";
                    }
                    catch (FormatException)
                    {
                        //Es tipo VARCHAR
                        tipoDato = "VARCHAR(100)";

                        if (Valor.Length > 100)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(DELETEoINT).Key + " campo VARCHAR(100) pero su tamaño es " + Valor.Length);
                        }
                    }
                }
            }
            return tipoDato;
        }
        public ActionResult Menu()
        {
            return View();
        }
        public ActionResult VerTablas()
        {
            return View(Datos.Instance.ListaTablaYValores);
        }
        public ActionResult IngresarSQL()
        {
            return RedirectToAction("Data");
        }
        public ActionResult Data()
        {
            return View("DatosSQL");
        }
        public ActionResult VerTablaSelect()
        {
            return View(Datos.Instance.ListaAMostrarSelect);
        }
        public void ConvertirEnLista()
        {
            Datos.Instance.ListaAMostrarSelect.Clear();
            string Nombre = Datos.Instance.NombreTabla;
            List<string> ListaNombreColumnas = Datos.Instance.NombreColumnasMostrar;
            List<List<object>> ListaFilasTabla = Datos.Instance.DatosParaMostrar;
            string NombreColumnas = "";
            var TablaAMandar = new ModeloTablaAMostrar();
            for (int i = 0; i < ListaNombreColumnas.Count; i++)
            {
                NombreColumnas += ListaNombreColumnas[i] + "|";
            }
            TablaAMandar.NombreTabla = Nombre;
            TablaAMandar.Elemento = NombreColumnas;
            Datos.Instance.ListaAMostrarSelect.Add(TablaAMandar);
            for (int i = 0; i < ListaFilasTabla.Count; i++)
            {
                string FilasTablas = "";
                var DatosMandar = new ModeloTablaAMostrar();
                for (int j = 0; j < ListaNombreColumnas.Count; j++)
                {
                    FilasTablas += ListaFilasTabla[i][j].ToString() + "|";
                }
                DatosMandar.Elemento = FilasTablas;
                Datos.Instance.ListaAMostrarSelect.Add(DatosMandar);
            }
        }
        public ActionResult ExportarCSV()
        {
            string Nombre = Datos.Instance.NombreTabla;
            string RutaArchivo = (@"~/microSQL/"+Nombre+".csv");
            try
            {
                var streamWriter = new StreamWriter(Server.MapPath(RutaArchivo), false);
                for (int i = 0; i < Datos.Instance.ListaAMostrarSelect.Count; i++)
                {
                    streamWriter.WriteLine(Datos.Instance.ListaAMostrarSelect[i].Elemento);
                }
                streamWriter.Flush();
                streamWriter.Close();
                Success(String.Format("Arhivo creado con exito en carpeta microSQL con el nombre"+Nombre+", retornando a Ingresar codigo"), true);
                return RedirectToAction("IngresarSQL");
            }
            catch (Exception)
            {

                Danger(string.Format("Hubo un error en la creacion de archivo, HAY UN ARCHIVO CON EL MISMO NOMBRE, retornando a Ingresar codigo"), true);
                return RedirectToAction("IngresarSQL");
            }
        }
        #endregion
    }
}
