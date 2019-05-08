using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trees_ED;

namespace ArbolBDLL
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
        public void CerrarArchivo()
        {
            fileTree.CerrarArchivo();
        }
        public override void ActualizarElemento(T ActuEle)
        {
            throw new NotImplementedException();
        }

        public override void AgregarElemento(T newData)
        {
            if (fileTree.RootIsEmpty(fileName))
            {
                List<TKey> lista = new List<TKey>();
                List<T> listaT = new List<T>();

                lista.Add(fileTree.knowTkey(newData));
                listaT.Add(newData);
                fileTree.ConstruirEncabezado(fileName, 0, 0, 0, order, 0);
                NodoArbolB<TKey, T> rootNode = new NodoArbolB<TKey, T>(listaT, new List<int>(), lista, -2147483648, 1, order);
                fileTree.InsertNode(rootNode, fileName, false);
                fileTree.ConstruirEncabezado(fileName, 1, 2, 1, order, 0);
            }
            else
            {
                tamaño++;
                AddElementTree(fileTree.getRoot(fileName), newData, fileTree.knowTkey(newData));
            }
        }

        public override List<object> ConvertirObjeto()
        {
            throw new NotImplementedException();
        }

        public override void EliminarElemento(T BorrarEle)
        {
            throw new NotImplementedException();
        }

        public override bool Vacio()
        {
            throw new NotImplementedException();
        }

        private void AddElementTree(int currentNode, T dato, TKey comparation)
        {
            NodoArbolB<TKey, T> currentBNode = fileTree.convertirLineaNodo(fileName, currentNode, order);
            if (currentBNode.NodoHoja())
            {
                for (int i = 0; i < order; i++)
                {
                    if (comparation.CompareTo(currentBNode.Llave.ElementAt(i)) == -1 || currentBNode.Llave.ElementAt(i) == null)
                    {
                        currentBNode.Llave.Insert(i, comparation);
                        currentBNode.Datos.Insert(i, dato);
                        break;
                    }

                }
                UnderFlow(currentBNode);
            }
            else
            {
                for (int i = 0; i < order; i++)
                {
                    if (comparation.CompareTo(currentBNode.Llave.ElementAt(i)) == -1 || currentBNode.Llave.ElementAt(i) == null)
                    {
                        AddElementTree(currentBNode.ApuntadorHijo.ElementAt(i), dato, comparation);
                        break;
                    }
                }
            }
        }
        private void UnderFlow(NodoArbolB<TKey, T> newB)
        {
            if (!newB.IsUnderflow())
            {
                fileTree.InsertNode(newB, fileName, true);
                fileTree.ConstruirEncabezado(fileName, fileTree.getRoot(fileName), fileTree.lastPosition(fileName), tamaño, order, 0);
                return;
            }
            if (order % 2 == 0)
            {
                factor = (order / 2) - 1;
            }
            else
            {
                factor = (order / 2);
            }
            int position = fileTree.lastPosition(fileName);
            if (newB.Padre == -2147483648)
            {
                NodoArbolB<TKey, T> sonRight = new NodoArbolB<TKey, T>();
                sonRight.Posicion = position;
                sonRight.Padre = position + 1;
                sonRight.Orden = order;
                for (int i = factor + 1; i <= order; i++)
                {
                    sonRight.ApuntadorHijo.Add(newB.ApuntadorHijo.ElementAt(i));
                    sonRight.Datos.Add(newB.Datos.ElementAt(i));
                    sonRight.Llave.Add(newB.Llave.ElementAt(i));
                }
                for (int i = 0; i < sonRight.ApuntadorHijo.Count; i++)
                {
                    if (sonRight.ApuntadorHijo.ElementAt(i) != -2147483648)
                    {
                        NodoArbolB<TKey, T> childrenParent = fileTree.convertirLineaNodo(fileName, sonRight.ApuntadorHijo.ElementAt(i), order);
                        childrenParent.Padre = sonRight.Posicion;
                        fileTree.InsertNode(childrenParent, fileName, true);
                    }
                }
                NodoArbolB<TKey, T> newRoot = new NodoArbolB<TKey, T>();

                newRoot.Posicion = position + 1;
                newRoot.Padre = -2147483648;
                newRoot.Orden = order;
                newRoot.ApuntadorHijo.Add(newB.Posicion);
                newRoot.ApuntadorHijo.Add(position);
                newRoot.Datos.Add(newB.Datos.ElementAt(factor));
                newRoot.Llave.Add(newB.Llave.ElementAt(factor));

                newB.Padre = position + 1;
                newB.ApuntadorHijo.RemoveRange(factor + 1, newB.ApuntadorHijo.Count - (factor + 1));
                newB.Datos.RemoveRange(factor, newB.Datos.Count - factor);
                newB.Llave.RemoveRange(factor, newB.Llave.Count - factor);

                fileTree.InsertNode(newB, fileName, true);
                fileTree.InsertNode(sonRight, fileName, false);
                fileTree.InsertNode(newRoot, fileName, false);

                fileTree.ConstruirEncabezado(fileName, newRoot.Posicion, position + 2, tamaño, order, 0);
            }
            else
            {
                NodoArbolB<TKey, T> parent = fileTree.convertirLineaNodo(fileName, newB.Padre, order);
                for (int i = 0; i < order; i++)
                {
                    try
                    {
                        if (parent.Llave.ElementAt(i).CompareTo(newB.Llave.ElementAt(factor)) == 1 || parent.Llave.ElementAt(i) == null)
                        {
                            parent.Llave.Insert(i, newB.Llave.ElementAt(factor));
                            parent.Datos.Insert(i, newB.Datos.ElementAt(factor));
                            parent.ApuntadorHijo.Insert(i + 1, position);
                            break;
                        }
                    }
                    catch (NullReferenceException)
                    {
                        parent.Llave.Insert(i, newB.Llave.ElementAt(factor));
                        parent.Datos.Insert(i, newB.Datos.ElementAt(factor));
                        parent.ApuntadorHijo.Insert(i + 1, position);
                        break;
                    }
                }
                NodoArbolB<TKey, T> sonRight = new NodoArbolB<TKey, T>();

                sonRight.Posicion = position;
                sonRight.Padre = parent.Posicion;
                sonRight.Orden = order;
                for (int i = factor + 1; i <= order; i++)
                {
                    sonRight.ApuntadorHijo.Add(newB.ApuntadorHijo.ElementAt(i));
                    sonRight.Datos.Add(newB.Datos.ElementAt(i));
                    sonRight.Llave.Add(newB.Llave.ElementAt(i));
                }
                for (int i = 0; i < sonRight.ApuntadorHijo.Count; i++)
                {
                    if (sonRight.ApuntadorHijo.ElementAt(i) != -2147483648)
                    {
                        NodoArbolB<TKey, T> childrenParent = fileTree.convertirLineaNodo(fileName, sonRight.ApuntadorHijo.ElementAt(i), order);
                        childrenParent.Padre = sonRight.Posicion;
                        fileTree.InsertNode(childrenParent, fileName, true);
                    }
                }
                newB.ApuntadorHijo.RemoveRange(factor + 1, newB.ApuntadorHijo.Count - (factor + 1));
                newB.Datos.RemoveRange(factor, newB.Datos.Count - factor);
                newB.Llave.RemoveRange(factor, newB.Llave.Count - factor);

                fileTree.InsertNode(newB, fileName, true);
                fileTree.InsertNode(sonRight, fileName, false);
                fileTree.InsertNode(parent, fileName, true);
                fileTree.ConstruirEncabezado(fileName, fileTree.getRoot(fileName), position + 1, tamaño, order, 0);
                UnderFlow(parent);
            }
        }




    }
}