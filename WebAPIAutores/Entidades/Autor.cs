using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.Validaciones;

namespace WebAPIAutores.Entidades
{
    public class Autor //: IValidatableObject //implemtentar validaciones propias de modelo
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]//para que un campo se obligatorio
        [StringLength(maximumLength: 20, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres")]// para que un campo no tenga mas de n caracteres
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }

        //[Range (8,70)]//rango de numeros
        //[NotMapped]//para hacer pruebas y se tenga que agregar a la base de datos   
        //public int edad { get; set; }
        //[CreditCard]//comprobar que es un a tarjeta de credito valida
        //[NotMapped]
        //public string TarjetaCredito { get; set; }
        //[Url]//Comprobar que sea una url
        //[NotMapped]
        //public string URL { get; set; }
        //[NotMapped]
        //public int Mayor { get; set; }
        //[NotMapped]
        //public int Menor { get; set; }

        //public List<Libro> Libros { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    if (!string.IsNullOrEmpty(Nombre))
        //    {
        //        var PrimeraLetra = Nombre[0].ToString();
        //        if (PrimeraLetra != PrimeraLetra.ToUpper())
        //        {
        //            yield return new ValidationResult("La primera Letra debe ser mayuscula", 
        //                new string[] { nameof(Nombre) }); //el yield agrega una nuevo elemento al la operacion IEnumerable
        //        }

        //    }

        //if (Menor > Mayor)
        //{
        //    yield return new ValidationResult("Este valor no puede ser mas grande que el campo Mayor",new string[] { nameof(Menor)});
        //}
        //}
    }
}
