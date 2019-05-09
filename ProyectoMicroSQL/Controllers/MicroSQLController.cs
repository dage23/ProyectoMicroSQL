﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text.RegularExpressions;
using System.Dynamic;
using ProyectoMicroSQL.Models;
using ProyectoMicroSQL.Controllers;
using ProyectoMicroSQL.Helpers;
using BTreeDLL;
using Tabla = BTreeDLL.Tabla;

namespace ProyectoMicroSQL.Controllers
{
    public class MicroSQLController : Controller
    {

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
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key))
                {
                    //Eliminar elemento
                    Eliminar(ArregloOperaciones[i]);
                }
                if (ArregloOperaciones[i].Contains(Datos.Instance.diccionarioColeccionada.ElementAt(5).Key))
                {
                    //Eliminar Archivo Tabla
                    DropTabla(ArregloOperaciones[i]);
                }      
            }
            return View("DatosSQL");
        }
        #endregion

        #region CREATE
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
            SepararParentesis[1] = SepararParentesis[1].Substring(0, SepararParentesis[1].Length - 1);
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
            Datos.Instance.ListaTablaYValores.Add(new Listado_Tablas { NombreTabla = NombreTabla, ValoresTabla = ValoresdeTabla, TipoValoresTabla= TipoValoresTabla });

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
            BTreeDLL.BTree<string, BTreeDLL.Tabla> CrearArbol = new BTreeDLL.BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreArbol + ".arbolb"), 8);
            CrearArbol.CloseStream();
        }
        #endregion

        #region INSERT
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
            BTreeDLL.BTree<string, BTreeDLL.Tabla> ArbolACrear = new BTree<string, Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTablaInsertar + ".arbolb"), 8);

            ArbolACrear.AddElement(TablaAInsertarEnArbol);
            ArbolACrear.CloseStream();
        }
        #endregion

        #region SELECT
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
                        ColumnaEnArchivo.Add(DatoActual.Split('|')[0]);
                        TipoDatoEnArchivo.Add(DatoActual.Split('|')[1]);
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
                string[] InstruccionesSeparadas = Regex.Split(Instucciones, Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
                if (InstruccionesSeparadas.Length == 1)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " no contiene " + Datos.Instance.diccionarioColeccionada.ElementAt(1).Key);
                }
                if (InstruccionesSeparadas.Length > 2)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " contiene " + Datos.Instance.diccionarioColeccionada.ElementAt(1).Key + " mas de una vez");
                }
                string[] ColumnasSolicitadas = InstruccionesSeparadas[0].Trim().Split(',');
                if (InstruccionesSeparadas[1].Trim().Split(' ').Length > 1)
                {
                    bool UlilizaWhere = false;
                    string NombreTabla = InstruccionesSeparadas[1].Trim().Split(' ')[0];

                    if (InstruccionesSeparadas[1].Trim().Split(' ')[1].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(3).Key)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " debe de tener " + Datos.Instance.diccionarioColeccionada.ElementAt(3).Key + " despues");
                    }
                    string[] InstruccionesSeparadas2 = Regex.Split(InstruccionesSeparadas[1].Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(3).Key);
                    InstruccionesSeparadas2[1] = InstruccionesSeparadas2[1].Trim();
                    string[] InstruccionesSeparadas3 = Regex.Split(InstruccionesSeparadas2[1].Trim(), "=");
                    if (InstruccionesSeparadas3.Length != 2)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee un error de condicion.");
                    }
                    if (InstruccionesSeparadas3[1].Trim() == "")
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee error despues de =.");
                    }
                    if (InstruccionesSeparadas3[0].Trim() == "")
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee un error antes de =.");
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
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " trata de buscar un valor nulo");
                        }
                        if (InstruccionesSeparadas3[1].Trim().Length != InstruccionesSeparadas3[1].Trim().Substring(0, ConteoFinal + 1).Length)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee valores adicionales no permitidos");
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
                    FileStream ArchivoDeTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Datos.Instance.NombreTabla = NombreTabla;
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
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + ", la columnaa buscar no existe");
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
                                    throw new FormatException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " desea buscar un valor VARCHAR de mas de 100 posiciones");
                                }
                            }
                        }
                    }
                    if (!ExisteTipoDato)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + ", desea  buscar un tipo de dato no reconocido");
                    }

                    if (tipoDato != TipoColumnaEnArchivo)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + ", desea  buscar un tipo de dato que no concuerda con la tabla");
                    }

                    if (tipoDato != Datos.Instance.ListaAtributos.ElementAt(1))
                    {
                        if (InstruccionesSeparadas3[1].Trim().Split(' ').Length > 1)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + ", posee un error de escritura, por favor revisar [columna=dato]");
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
                                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee columnas repetidas");
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
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee una columna mal colocada, por favor revisa el orden de insercion de codigo");
                        }
                        ArregloColumnaRepetidas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    //-------------------------------select columna from where metodo4-------------------
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnasSolicitadas[i] = ColumnasSolicitadas[i].Trim();
                    }
                    BTreeDLL.BTree<string, BTreeDLL.Tabla> CrearArbol = new BTree<string, Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTabla + ".arbolb"), 8);
                    Datos.Instance.NombreTabla = NombreTabla;
                    List<BTreeDLL.Tabla> Listadatos = CrearArbol.goOverTreeInOrder();
                    List<BTreeDLL.Tabla> DatosTabla = new List<Tabla>();
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
                    TablaAMostrar tablaAMostrar = new TablaAMostrar();
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
                    for (int i = 0; i < tablas.Length; i++)
                    {
                        if (tablas[i] == nombreTabla.ToUpper())
                        {
                            ExisteTabla = true;
                        }
                    }
                    if (!ExisteTabla)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + ", la tabla no existe.");
                    }
                    FileStream tabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + nombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    Datos.Instance.NombreTabla = nombreTabla;
                    StreamReader Lector = new StreamReader(tabla);
                    ColumnaEnArchivo = new List<string>();
                    TipoDatoEnArchivo = new List<string>();
                    string[] ArregloColumnasRepetidas = new string[ColumnasSolicitadas.Length];
                    string TipoActual;
                    Lector.ReadLine();
                    for (int i = 0; i < ArregloColumnasRepetidas.Length; i++)
                    {
                        ArregloColumnasRepetidas[i] = "";
                    }
                    while ((TipoActual=Lector.ReadLine())!=null)
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
                            if (ColumnasSolicitadas[i].Trim().ToUpper()==ArregloColumnasRepetidas[j].Trim())
                            {
                                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key+" posee una columna repetida");
                            }
                        }
                        for (int k = 0; k < ColumnaEnArchivo.Count; k++)
                        {
                            if (ColumnasSolicitadas[i].Trim().ToUpper()==ColumnaEnArchivo.ElementAt(k))
                            {
                                ExisteTodasColumnas = true;break;
                            }
                        }
                        if (!ExisteTodasColumnas)
                        {
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(0).Key + " posee una columna que no esta en la posicion correcta");
                        }
                        ArregloColumnasRepetidas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    for (int i = 0; i < ColumnasSolicitadas.Length; i++)
                    {
                        ColumnasSolicitadas[i] = ColumnasSolicitadas[i].ToUpper();
                    }
                    BTreeDLL.BTree<string, BTreeDLL.Tabla> CrearArbol = new BTree<string, Tabla>(Server.MapPath(@"~/microSQL/arbolesb/"+nombreTabla+".arbolb"),8);
                    Datos.Instance.NombreTabla = nombreTabla;
                    List<BTreeDLL.Tabla> datosTablas = CrearArbol.goOverTreeInOrder();
                    TablaAMostrar tablaAMostrar = new TablaAMostrar();
                    tablaAMostrar.NombreColumnasArchivo = ColumnaEnArchivo;
                    tablaAMostrar.NombreColumnasAMostrar = ColumnasSolicitadas.ToList();
                    tablaAMostrar.ListaDatos = datosTablas;
                    tablaAMostrar.DatosAMostrarSelect();
                    CrearArbol.CloseStream();
                }
            }
            ConvertirEnLista();
        }
        #endregion

        #region  Drop
        //-----------------------------Función de SQL que borra una tabla de MiniSQL-----------------------------
        public void DropTabla(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(5).Key, "");//Quita la palabra reservada para la funciónn
            if (Valor.Trim().Split(' ').Length > 1)//Se comprueba que se tenga solo el nombre de la tabla que se eliminará
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(5).Key + " debe de poseer el nombre de la tabla que se eliminará");
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
                throw new InvalidOperationException("El nombre de la tabla a eliminar no existe");
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

        }
        #endregion

        #region Eliminar
        public void Eliminar(string Valor)
        {
            Valor = Valor.Replace(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key, "").Trim();//Eliminar la palabra reservada para la acción
            if (Valor.Split(' ').Length < 2)//Comprueba que tenga almenos 2 campos
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " está incompleto");
            }
            if (Valor.Split(' ')[0].Trim() != Datos.Instance.diccionarioColeccionada.ElementAt(1).Key)//Sintaxis erronea o no completa
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " sintaxis incorrecta o incompleta");
            }
            string NombreTabla = Valor.Split(' ')[1].Trim();
            string[] Tablas = Datos.Instance.ListaTablasExistentes.ToArray();
            bool ExistenciaTabla = false;//Verificar que la tabla exista
            for (int i = 0; i < Tablas.Length; i++)
            {
                if (Tablas[i] == NombreTabla)
                {
                    ExistenciaTabla = true;
                }
            }
            if (!ExistenciaTabla)
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " la tabla no existe");
            }
            if (Valor.Split(' ').Length < 2)//Se comprueba si contiene WHERE
            {
                if (Valor.Split(' ')[2] != Datos.Instance.diccionarioColeccionada.ElementAt(3).Key)//Cuando tenga
                {
                    throw new InvalidOperationException("Despues de " + Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " debe de ir " + Datos.Instance.diccionarioColeccionada.ElementAt(3).Key);
                }
            }
            string[] DivValor2 = Regex.Split(Valor.Trim(), Datos.Instance.diccionarioColeccionada.ElementAt(3).Key);
            DivValor2[1] = DivValor2[1].Trim();

            //---------------------------Posibles errores de sintaxis---------------------------
            string[] DivValor3 = Regex.Split(DivValor2[1].Trim(), "=");//ID=1
            if (DivValor3.Length != 2)
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " error en la condición");
            }
            if (DivValor3[0].Trim() == "")
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " no hay nada del lado izquierdo a la igualación");
            }
            if (DivValor3[1].Trim() == "")
            {
                throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " no hay nada del lado derecho a la igualación");
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
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " buscador en WHERE es nulo");
                }
                if (DivValor3[1].Trim().Length != DivValor3[1].Trim().Substring(0, Final + 1).Length)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + ", por favor escribir solamente [Columna = Dato]");
                }
                DivValor3[1] = DivValor3[1].Trim().Substring(0, Final + 1);
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
                FileStream GTabla = new FileStream(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".tabla"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamReader Lectura = new StreamReader(GTabla);
                List<string> ColumnaArchivo = new List<string>();
                List<string> Formato = new List<string>();
                string Tipo;
                Lectura.ReadLine();
                while ((Tipo = Lectura.ReadLine()) != null)
                {
                    ColumnaArchivo.Add(Tipo.Split('|')[0]);
                    Formato.Add(Tipo.Split('|')[1]);
                }
                GTabla.Close();

                VarChar = false;
                string TipoColumArchivo = "";
                int ColumPos = -1;

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
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " la columna no existe");
                }
                VarChar = false;

                string TipoDato = "";
                if (TipoDato != TipoColumArchivo)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " los campos en la condición no tienen el mismo tipo que la columna");
                }
                if (TipoDato != Datos.Instance.ListaAtributos.ElementAt(2))
                {
                    if (DivValor3[1].Trim().Split(' ').Length > 1)
                    {
                        throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + ", por favor escribir [Columna = Dato]");
                    }
                }
                VarChar = false;

                BTreeDLL.BTree<string, BTreeDLL.Tabla> ArbolACrear = new BTree<string, Tabla>(Server.MapPath(@"~/microSQL/arbolesb/" + NombreTabla + ".arbolb"), 8);
                List<BTreeDLL.Tabla> DatosLista = ArbolACrear.goOverTreeInOrder();
                BTreeDLL.Tabla TablaEliminar = new Tabla(int.Parse(DatoVarchar), null);
                if (ArbolACrear.SearchElementTree(TablaEliminar) == null)
                {
                    throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " el ID no existe en el arbol");
                }
                if (Columna == "ID")//Eliminar elemento por ID
                {
                    BTreeDLL.Tabla DatoEliminar = new BTreeDLL.Tabla(int.Parse(DatoVarchar), null);
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
                                    DatoVarchar = DatoVarchar.Replace("'","");
                                    if (j == ColumPos && DatosLista.ElementAt(i).Objetos.ElementAt(j).ToString().Replace("#", "") == DatoVarchar)
                                    {
                                        BTreeDLL.Tabla Eliminar_VARCHAR = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
                                        ArbolACrear.DeleteElement(Eliminar_VARCHAR);
                                    }
                                    break;
                                case "INT":
                                    if (j == ColumPos && Convert.ToInt32(DatosLista.ElementAt(i).Objetos.ElementAt(j)) == int.Parse(DatoVarchar))
                                    {
                                        BTreeDLL.Tabla Eliminar_INT = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
                                        ArbolACrear.DeleteElement(Eliminar_INT);
                                    }
                                    break;
                                case "DATETIME":
                                    BTreeDLL.Tabla Eliminar_DATETIME = new BTreeDLL.Tabla(DatosLista.ElementAt(i).ID, null);
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
                BTreeDLL.BTree<string, BTreeDLL.Tabla> ArbolCrear = new BTree<string, BTreeDLL.Tabla>(Server.MapPath(@"~/microSQL/tablas/" + NombreTabla + ".tabla"), 8);
                ArbolCrear.CloseStream();
            }
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
                            throw new InvalidOperationException(Datos.Instance.diccionarioColeccionada.ElementAt(2).Key + " campo VARCHAR(100) pero su tamaño es " + Valor.Length);
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
        public RedirectResult RedirectToAspx()
        {
            return Redirect("/WebForm1.aspx");
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
        #endregion
    }
}







