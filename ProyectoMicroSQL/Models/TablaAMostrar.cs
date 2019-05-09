using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProyectoMicroSQL.Helpers;
namespace ProyectoMicroSQL.Models
{
    class TablaAMostrar
    {
        public List<string> NombreColumnasArchivo { get; set; }
        public List<string> NombreColumnasAMostrar { get; set; }
        public List<BTreeDLL.Tabla> ListaDatos { get; set; }
        public List<List<object>> ListaDatosOrdenada { get; set; }
        public List<object> ListaObjetos { get; set; }
        public void DatosAMostrarSelect()
        {
            ListaDatosOrdenada = new List<List<object>>();
            for (int i = 0; i < ListaDatos.Count; i++)
            {
                ListaObjetos = new List<object>();
                for (int j = 0; j < NombreColumnasAMostrar.Count; j++)
                {
                    if (NombreColumnasAMostrar.ElementAt(j).Trim() == "ID")
                    {
                        ListaObjetos.Add(ListaDatos.ElementAt(i).ID);
                    }
                    else
                    {
                        for (int k = 0; k < NombreColumnasArchivo.Count; k++)
                        {
                            if (NombreColumnasArchivo.ElementAt(k).Trim() == NombreColumnasAMostrar.ElementAt(j).Trim())
                            {
                                ListaObjetos.Add(ListaDatos.ElementAt(i).Objetos.ElementAt(k - 1).ToString().Replace("#", ""));
                            }
                        }
                    }
                }
                ListaDatosOrdenada.Add(ListaObjetos);
            }
            Datos.Instance.DatosParaMostrar = ListaDatosOrdenada;
            Datos.Instance.NombreColumnasMostrar = NombreColumnasAMostrar;
        }
    }
}