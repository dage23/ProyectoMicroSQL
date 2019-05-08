using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArbolBDLL
{
    public class Tabla : IComparable<Tabla>
    {
        public Tabla(int id, List<object> objetos)
        {
            ID = id;
            Objetos = objetos;
        }
        public int ID { get; set; }
        public List<object> Objetos { get; set; }
        public int CompareTo(Tabla other)
        {
            if (ID > other.ID)
            {
                return 1;
            }
            else
            {
                if (ID == other.ID)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
        }

    }
}