using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProyectoMicroSQL.Models;
public class Datos
{
    public List<List<object>> DatosParaMostrar { get; set; }
    public List<string> NombreColumnasMostrar { get; set; }
    public string NombreTabla { get; set; }

    private static Datos _instance = null;
    public static Datos Instance
    {
        get
        {
            if (_instance == null) _instance = new Datos();
            {
                return _instance;
            }
        }
    }
    public Dictionary<string, string> diccionarioColeccionada = new Dictionary<string, string>();
    public List<string> ListaAtributos = new List<string>();
    public List<string> ListaTablasExistentes = new List<string>();
    public List<Listado_Tablas> ListaTablaYValores = new List<Listado_Tablas>();
}

