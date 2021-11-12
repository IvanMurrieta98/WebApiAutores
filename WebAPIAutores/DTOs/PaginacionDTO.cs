using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 10;
        private readonly int CantidadMaximaPorPagina = 50;

        public int RecordsPorPagina
        {
            get 
            {
                return recordsPorPagina;
            }
            set
            {
                recordsPorPagina = (value > CantidadMaximaPorPagina) ? CantidadMaximaPorPagina : value;
            }
        }
    }
}
