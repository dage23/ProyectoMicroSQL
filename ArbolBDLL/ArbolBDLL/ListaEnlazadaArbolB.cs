using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArbolBDLL
{
    class ListaEnlazadaArbolB<T> where T: IComparable<T>
    {
        private NodoListaEnlazada<T> Cabeza;
        private NodoListaEnlazada<T> Cola;
        private int Tam { get; set; }

        public ListaEnlazadaArbolB()
        {
            Tam = 0;
            Cabeza = null;
            Cola = null;
        }

        public ListaEnlazadaArbolB(ListaEnlazadaArbolB<T> newList)
        {
            Cabeza = newList.Cabeza;
        }

        public void Insertar(T model)
        {
            NodoListaEnlazada<T> newElement = new NodoListaEnlazada<T>(model);
            if (Cabeza == null)
            {
                Cabeza = newElement;
                Cola = newElement;
            }
            else
            {
                newElement.Siguiente = Cabeza;
                Cabeza = newElement;
            }
        }

        public void InsertarUltimo(T model)
        {
            NodoListaEnlazada<T> newElement = new NodoListaEnlazada<T>(model);
            if (Cabeza == null)
            {
                Cabeza = newElement;
                Cola = newElement;
            }
            else
            {
                Cola.Siguiente = newElement;
                Cola = newElement;
            }
            Tam++;
        }

        public int TamanoLista()
        {
            return Tam;
        }

        public void Borrar()
        {
            if (Cabeza != null)
            {
                NodoListaEnlazada<T> current = new NodoListaEnlazada<T>();
                current = Cabeza;

                while (current.Siguiente != null)
                {
                    current = current.Siguiente;
                }

                current = null;
                Tam--;
            }
        }

        public T ElementoAt(int index)
        {
            if (Cabeza == null)
            {
                return default(T);
            }
            else
            {
                NodoListaEnlazada<T> current = new NodoListaEnlazada<T>();
                current = Cabeza;

                while (index != 0)
                {
                    current = current.Siguiente;
                    index--;
                }
                return current.Valor;
            }
        }
    }
}
