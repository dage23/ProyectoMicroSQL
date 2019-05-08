using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArbolBDLL
{
    public class NodoListaEnlazada<T>
    {
        public T Valor {get; set;}
        public NodoListaEnlazada<T> Siguiente { get; set; }

        public NodoListaEnlazada()
        {
            Valor = default(T);
            Siguiente = null;
        }
        public NodoListaEnlazada(T value) 
        {
            this.Valor = value;
            Siguiente = null;
        }
    }
}