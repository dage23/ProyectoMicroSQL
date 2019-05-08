using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace ProyectoMicroSQL.Helpers
{
    public class Utilidades
    {
        public List<List<object>> DatosParaMostrar { get; set; }
        public List<string> NombreColumnasMostrar { get; set; }
        public string NombreTabla { get; set; }

    }
}