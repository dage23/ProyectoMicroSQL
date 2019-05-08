using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArbolBDLL
{
    public class FileHandler<TKey, T> where TKey : IComparable<TKey> where T : IComparable<T>
    {
        FileStream archivoStream;
        const long tamañoEncabezado = 65;
        long positionLine = 0;

        public FileHandler(string fileName, int orden)
        {
            archivoStream = new FileStream(fileName+".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            archivoStream.Close();
        }

        public TKey knowTkey(T objeto)
        {
            if (typeof(T) == typeof(string))
            {
                return (TKey)(object)objeto.ToString();
            }
            return default(TKey);
        }

        public void ConstruirEncabezado(string fileName, int raiz, int posicion, int tamaño, int orden, int altura)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            StreamWriter escribirEncabezado = new StreamWriter(archivoStream);
            escribirEncabezado.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(raiz));
            escribirEncabezado.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(posicion));
            escribirEncabezado.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(tamaño));
            escribirEncabezado.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(orden));
            escribirEncabezado.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(altura));
            escribirEncabezado.Close();
        }
        public void InsertNode(NodoArbolB<TKey, T> nodo, string fileName, bool update)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            positionLine = tamañoEncabezado + (nodo.Posicion - 1) * (34 + (11 * nodo.Orden + (nodo.Orden - 1)) + (36 * (nodo.Orden - 1) + (nodo.Orden - 2)) + (36 * (nodo.Orden - 1) + (nodo.Orden - 1)));
            archivoStream.Seek(positionLine, SeekOrigin.Begin);
            StreamWriter escribirNodo = new StreamWriter(archivoStream);
            if (update == true)
            {
                escribirNodo.Write(Textos<TKey, T>.getInsancia().FormatoInt(nodo.Posicion) + "|" +
                Textos<TKey, T>.getInsancia().FormatoInt(nodo.Padre) + "|||" +
                Textos<TKey, T>.getInsancia().LisatHijos(nodo.ApuntadorHijo, nodo.Orden) + "||" +
                Textos<TKey, T>.getInsancia().fabricarListaKeys(nodo.Llave, nodo.Orden) + "||" +
                Textos<TKey, T>.getInsancia().fabricarListaAtributos(nodo.Datos, nodo.Orden));
            }
            else
            {
                escribirNodo.WriteLine(Textos<TKey, T>.getInsancia().FormatoInt(nodo.Posicion) + "|" +
                Textos<TKey, T>.getInsancia().FormatoInt(nodo.Padre) + "|||" +
                Textos<TKey, T>.getInsancia().LisatHijos(nodo.ApuntadorHijo, nodo.Orden) + "||" +
                Textos<TKey, T>.getInsancia().fabricarListaKeys(nodo.Llave, nodo.Orden) + "||" +
                Textos<TKey, T>.getInsancia().fabricarListaAtributos(nodo.Datos, nodo.Orden));
            }
            escribirNodo.Close();
        }

        public NodoArbolB<TKey, T> convertirLineaNodo(string fileName, int position, int orden)
        {
            if (position <= -2147483648)
            {
                return null;
            }
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            positionLine = tamañoEncabezado + (position - 1) * (34 + (11 * orden + (orden - 1)) + (36 * (orden - 1) + (orden - 2)) + (36 * (orden - 1) + (orden - 1)));
            archivoStream.Seek(positionLine, SeekOrigin.Begin);
            StreamReader lectura = new StreamReader(archivoStream);
            string line = lectura.ReadLine();
            string[] lineaArray = line.Split('|');
            lectura.Close();

            int posicionNodo = int.Parse(lineaArray[0]);
            int padreNodo = int.Parse(lineaArray[1]);
            int iH = 4;
            int iL = iH + orden + 2;
            int iD = iL + (orden - 1) + 2;
            int fin = iD + orden - 1;

            List<int> hijos = new List<int>();
            for (int i = iH; i < iL - 2; i++)
            {
                hijos.Add(int.Parse(lineaArray[i]));
            }
            hijos.Add(-2147483648);

            List<TKey> llaves = new List<TKey>();
            for (int i = iL; i < iD - 2; i++)
            {
                if (lineaArray[i].Contains("#"))
                {
                    llaves.Add((TKey)(object)null);
                }
                else
                {
                    llaves.Add((TKey)(object)lineaArray[i]);
                }
            }

            llaves.Add((TKey)(object)null);

            List<T> datos = new List<T>();
            for (int i = iD; i < fin; i++)
            {
                if (lineaArray[i].Contains("#"))
                {
                    datos.Add((T)(object)null);
                }
                else
                {
                    datos.Add((T)(object)lineaArray[i]);
                }
            }
            datos.Add((T)(object)null);
            NodoArbolB<TKey, T> newNode = new NodoArbolB<TKey, T>(datos, hijos, llaves, padreNodo, posicionNodo, orden);
            return newNode;
        }

        public long ObtenerPosicionArchivo()
        {
            return 0;
        }

        public bool RootIsEmpty(string fileName)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader leerEncabezado = new StreamReader(archivoStream);
            if (leerEncabezado.ReadLine() == "00000000000" || leerEncabezado.ReadLine() == null)
            {
                leerEncabezado.Close();
                return true;
            }
            leerEncabezado.Close();
            return false;
        }

        public int lastPosition(string fileName)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            archivoStream.Seek(16, SeekOrigin.Begin);
            StreamReader leerEncabezado = new StreamReader(archivoStream);
            int last = int.Parse(leerEncabezado.ReadLine());
            leerEncabezado.Close();
            return last;
        }

        public int getRoot(string fileName)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamReader leerEncabezado = new StreamReader(archivoStream);
            int raiz = int.Parse(leerEncabezado.ReadLine());
            leerEncabezado.Close();
            return raiz;
        }

        public int getTamaño(string fileName)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            archivoStream.Seek(29, SeekOrigin.Begin);
            StreamReader leerEncabezado = new StreamReader(archivoStream);

            int tamaño = int.Parse(leerEncabezado.ReadLine());
            leerEncabezado.Close();
            return tamaño;
        }
        public int getAltura(string fileName)
        {
            archivoStream = new FileStream(fileName + ".arbolb", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            archivoStream.Seek(55, SeekOrigin.Begin);
            StreamReader leerEncabezado = new StreamReader(archivoStream);
            int altura = int.Parse(leerEncabezado.ReadLine());
            leerEncabezado.Close();
            return altura;
        }
        public NodoArbolB<TKey, T> ObtenerNodo()
        {
            return null;
        }

        public void CerrarArchivo()
        {
            archivoStream.Close();
        }
    }
}