using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoMicroSQL.Models
{   
    public abstract class ArbolAbstracto<T> where T : IComparable<T>
    {
        public abstract void AgregarElemento(T NuevoDato);

        public abstract void EliminarElemento(T BorrarEle);

        public abstract void ActualizarElemento(T ActuEle);

        public abstract bool isEmpty();

        public abstract List<Object> ConvertirObjeto();
    }    
}