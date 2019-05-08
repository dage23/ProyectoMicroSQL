using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ProyectoMicroSQL.Controllers;
using ProyectoMicroSQL.Models;
using ProyectoMicroSQL.Helpers;

namespace ProyectoMicroSQL
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void GridView1_SelectedIndexChanged(List<string> ColumnaNombre, List<List<object>> Filas)
        {
            DataTable dataTable = new DataTable();//Se inicializa
            for (int i = 0; i < ColumnaNombre.Count; i++)//Agrega datos a las Columnas
            {
                dataTable.Columns.Add(ColumnaNombre[i]);
            }            
            for (int i = 0; i < Filas.Count; i++)//Agrega los datos a las Filas
            {
                string[] ActualFila = new string[ColumnaNombre.Count];
                for (int j = 0; j < ColumnaNombre.Count; j++)
                {
                    ActualFila[j] = Filas[i][j].ToString();
                }
                dataTable.Rows.Add(ActualFila);
            }
        }

        protected void GridView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}