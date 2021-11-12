using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.Validaciones;

namespace WebAPIAutores.Entidades
{
    public class Libro
    {
        public int Id { get; set; }
        [Required]
        [PrimeraLetraMayuscula]
        [StringLength(maximumLength: 150)]
        public string Titulo { get; set; }
        public DateTime? FechaPublicacion { get; set; } //con el signo ? hacemos que el campo pueda ser nulo, agregamos una nueva migracion
        public List<Comentario> Comentarios { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }
        //public int AutorId { get; set; }
        //public Autor Autor { get; set; } // este nos sirve paraacceder a la entidad del autor al momento de agregar un libro

    }
}
