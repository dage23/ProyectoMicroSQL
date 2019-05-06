using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class Datos
{
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
}

