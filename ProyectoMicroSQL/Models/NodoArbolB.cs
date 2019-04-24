using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoMicroSQL.Models
{
    public class NodoArbolB<TKey, T>
    {
        public List<T> Datos { get; set; }
        public List<int> ApuntadorHijo { get; set; }
        public List<TKey> Llave { get; set; }
        public int Padre { get; set; }
        public int Orden { get; set; }
        public int Posicion { get; set; }
        public NodoArbolB(List<T> Datos, List<int> ApuntadorHijo, List<TKey> Llave, int Padre, int Orden, int Posicion)
        {
            this.Datos = Datos;
            this.ApuntadorHijo = ApuntadorHijo;
            this.Llave = Llave;
            this.Padre = Padre;
            this.Posicion = Posicion;
            this.Orden = Orden;
        }
        public NodoArbolB()
        {
            Datos = new List<T>();
            ApuntadorHijo = new List<int>();
            Llave = new List<TKey>();
        }
        public bool NodoHoja()
        {
            int count = 0;
            for (int i = 0; i < Orden; i++)
            {
                if (ApuntadorHijo.ElementAt(i) == -2147483648)
                    count++;
            }
            return count == Orden ? true : false;
        }
        public bool Underflow()
        {
            int count = 0;
            for (int i = 0; i < Orden; i++)
            {
                if (Llave.ElementAt(i) == null)
                    count++;
            }

            return count == 0;
        }
        public int ValoresHijo()
        {
            int sum = 0;
            for (int i = 0; i < ApuntadorHijo.Count; i++)
            {
                if (ApuntadorHijo[i] > -2147483648)
                    sum++;
            }
            return sum;
        }
        public int ValoresDatos()
        {
            int sum = 0;
            for (int i = 0; i < Datos.Count; i++)
            {
                if (Datos[i] != null)
                    sum++;
            }
            return sum;
        }
        public int ValoresLlaves()
        {
            int sum = 0;
            for (int i = 0; i < Llave.Count; i++)
            {
                if (Llave[i] != null)
                    sum++;
            }
            return sum;
        }
    }
}