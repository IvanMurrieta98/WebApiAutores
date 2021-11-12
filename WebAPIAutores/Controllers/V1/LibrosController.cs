using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/Libros")]//ruta del controlador
    public class LibrosController : ControllerBase//para agregar la libreria de microsoft MVC
    {
        private readonly AplicationDBContext context;
        private readonly IMapper mapper;

        public LibrosController(AplicationDBContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDTOConAutores>> Get(int id)
        {
            var libro = await context.Libros.Include(libroDB => libroDB.AutoresLibros)//este comando incluye a la tabla autores libros para acceder a ella
                .ThenInclude(autorlibroDB => autorlibroDB.Autor)//este comando incluye a la data del autor que se encuentra de dentro de AutoresLibros
                                                                //Include(libroBD => libroBD.Comentarios)//kace un join entre las tablas
                .FirstOrDefaultAsync(librosBD => librosBD.Id == id); //retorna el primer dato que se relacione
            if (libro == null)
            {
                return NotFound();
            }
            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();
            return mapper.Map<LibroDTOConAutores>(libro);

        }

        [HttpPost(Name ="crearLibro")]
        public async Task<ActionResult> Post(LibroCreacionDTO libroCreacionDTO)
        {
            //var existeAutor = await context.Autores.AnyAsync(x => x.Id == libro.AutorId);
            //if (!existeAutor)
            //{
            //    return BadRequest($"No existe el autor de Id: {libro.AutorId}");
            //}

            if (libroCreacionDTO.AutoresIds == null)
            {
                return BadRequest("No se puede crear libros sin autoress");
            }
            var autoresIds = await context.Autores.Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.Id))//Verifica que el eutor que ingreso exista en la base de datos
                .Select(x => x.Id).ToListAsync();//SElecciona el autor que encontro
            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }
            var existeMismoNombre = await context.Libros.AnyAsync(libroDB => libroDB.Titulo == libroCreacionDTO.Titulo);
            if (existeMismoNombre)
            {
                return BadRequest($"Ya existe un libro con el titulo: {libroCreacionDTO.Titulo}");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutores(libro);


            context.Add(libro);
            await context.SaveChangesAsync();

            var libroDTO = mapper.Map<LibroDTO>(libro);
            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDTO);
        }

        [HttpPut("id: int",Name ="actualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDTO libroCreacionDTO)
        {
            var libroDB = await context.Libros
                .Include(x => x.AutoresLibros)//Incluimos el listado  AutoresLibros
                .FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            libroDB = mapper.Map(libroCreacionDTO, libroDB); //Asiganmos la informacion que tenemos en librocreacionDTO a libroDB
            AsignarOrdenAutores(libroDB);
            await context.SaveChangesAsync();
            return NoContent();


        }

        private void AsignarOrdenAutores(Libro libro)
        {

            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }

        [HttpPatch("{id:int}",Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDTO> patchDocument) //el JsonPatchDocument es para actualizar solo una parte del objeto
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
            var libroDB = await context.Libros.FirstOrDefaultAsync(x => x.Id == id);
            if (libroDB == null)
            {
                return NotFound();
            }
            var libroDTO = mapper.Map<LibroPatchDTO>(libroDB); // llenamos el LibroPatchDTO con la informacion de libroDB
            patchDocument.ApplyTo(libroDTO, ModelState); //aplicamos los cambios que ocurrieron en el patchDocument

            var EsValido = TryValidateModel(libroDTO); //validamos el libroDTO
            if (!EsValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(libroDTO, libroDB); //PASAMOS LA INFORMACION DE libroDTO hacia el libroDB
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}",Name ="eliminarLibro")]//para eliminar un libro, se combina la ruta del controlador (Route) + el id (api/libros/id)
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id); //AnyAsyn nos sirve para si ver si existe el objeto
            if (!existe)
            {
                return NotFound(); // nos devuelve un Error 404
            }
            context.Remove(new Libro() { Id = id }); //marcamos al autor con remove para despues eliminarlo 
            await context.SaveChangesAsync();
            return NoContent();
        }


    }
}
