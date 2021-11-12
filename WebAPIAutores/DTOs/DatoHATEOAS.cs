using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.DTOs
{
    public class DatoHATEOAS
    {
        public string Enlace { get; private set; }
        public string Descripcion { get; private set; }
        public string Metodo { get; private set; }
        //agregamos private para que solo puedamos crear y no modificar los datos

        public DatoHATEOAS(string enlace,string descripcion,string metodo)
        {
            Enlace = enlace;
            Descripcion = descripcion;
            Metodo = metodo;
        }

        //CReamos una clase base para nuestros DTOs que necesitan utilizar el HATEOAS

    }
}
