using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoMicroSQL.Models
{
    public class Textos<TKey, T> where TKey : IComparable<TKey> where T : IComparable<T>
    {
        private static Textos<TKey, T> instancia;

        public static Textos<TKey, T> getInsancia()
        {
            return instancia ?? (instancia = new Textos<TKey, T>());
        }

        public string FormatoInt(int Num)
        {
            return Num == -2147483648 ? "-2147483648" : Num.ToString("00000000000");
        }

        public string LisatHijos(List<int> hijos, int orden)
        {
            string format = "";
            int count = 0;
            for (int i = 0; i < orden; i++)
            {
                try
                {
                    if (hijos.ElementAt(i) > -2147483648)
                    {
                        count++;
                        format += FormatoInt(hijos.ElementAt(i)) + "|";
                    }
                }
                catch
                {

                }
            }
            int difference = orden - count;
            for (int j = 0; j < difference; j++)
            {
                format += "-2147483648|";
            }
            return format;
        }

        public string fabricarListaKeys(List<TKey> llaves, int orden)
        {
            string format = "";
            for (int i = 0; i < (orden - 1); i++)
            {
                try
                {
                    format += llaves.ElementAt(i).ToString() + "|";
                }
                catch (Exception)
                {
                    format += "#########|";//9Datos
                }
            }
            return format;
        }

        public string fabricarListaAtributos(List<T> objeto, int orden)
        {
            string format = "";
            for (int i = 0; i < (orden - 1); i++)
            {
                try
                {
                    format += objeto.ElementAt(i).ToString() + "|";
                }
                catch (Exception)
                {
                    format += "#########|";//9Datos
                }
            }
            return format;
        }

        public int CountAttributes(T objeto)
        {
            return objeto.GetType().GetProperties().Length;
        }

    }
}