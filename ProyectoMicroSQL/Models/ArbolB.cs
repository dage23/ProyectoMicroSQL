using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProyectoMicroSQL.Models;

namespace ProyectoMicroSQL.Models
{
    public class ArbolB<TKey, T> : ArbolAbstracto<T> where TKey : IComparable<TKey> where T : IComparable<T>
    {
        private const int MINORDER = 3;
        private const int MAXORDER = 99;

        private Queue<int> DeletedParentsP = new Queue<int>();
        private Queue<int> DeletedIndex = new Queue<int>();

        private NodoArbolB<TKey, T> root;
        private string fileName;
        private int order;
        private int tamaño;
        private FileHandler<TKey, T> fileTree;
        private int factor;

        public ArbolB(string fileName, int order)
        {
            if (order <= MAXORDER && order >= MINORDER)
            {
                this.order = order;
                this.fileName = fileName;
                root = null;
                fileTree = new FileHandler<TKey, T>(fileName, order);
                tamaño = 0;
            }
        }



    }
}