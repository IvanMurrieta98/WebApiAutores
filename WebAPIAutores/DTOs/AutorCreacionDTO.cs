using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.Validaciones;

namespace WebAPIAutores.DTOs
{
    public class AutorCreacionDTO
    {
        [Required(ErrorMessage = "El campo {0} es requerido")]//para que un campo se obligatorio
        [StringLength(maximumLength: 20, ErrorMessage = "El campo {0} no debe tener más de {1} carácteres")]// para que un campo no tenga mas de n caracteres
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
       



        //Instalar en el administrador de paquetes nuget el AutoMapper.Extensions.Microsoft.DenpendencyInjection para hacer un mapeo de las propiedades 
    }
}
