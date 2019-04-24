using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoMicroSQL.Models
{
    public abstract class ArbolAbstracto<T> where T : IComparable<T>
    {
        //METHODS USEFULL FOR THE TREE'S ACTIONS
        /// <summary>
        /// Adds the T elementData to a new TreeNode
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public abstract void AddElement(T newData);


        /// <summary>
        /// Deletes a TreeNode containing the elementDeletingData 
        /// </summary>
        /// <param name="order"></param>
        /// <param name="EliminarElemento"></param>
        /// <returns></returns>
        public abstract void DeleteElement(T EliminarElemento);


        /// <summary>
        /// Changes the data of a TreeNode with the elementUpdatingData
        /// Both have the same id, only change the other atributes
        /// </summary>
        /// <param name="order"></param>
        /// <param name="newElementData"></param>
        /// <returns></returns>
        public abstract void UpdateElement(T ActualizarElemento);


        /// <summary>
        /// Returns true if the tree has no root (doesn't have treeNodes)
        /// </summary>
        /// <returns></returns>
        public abstract bool isEmpty();


        /// <summary>
        /// Converts the tree into a .NetList
        /// </summary>
        /// <returns></returns>
        public abstract List<Object> converToObject();
    }
    
}