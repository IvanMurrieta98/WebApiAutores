using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;
using WebAPIAutores.Utilidades;

namespace WebAPIAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly AplicationDBContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        //Creamos un constructor para tener acceso a nuestro aplicationBDcontext
        public ComentariosController(AplicationDBContext context, IMapper mapper
            ,UserManager<IdentityUser> userManager         //Inyectamos un servicio de Identity que nos permitira obtener el id de los usuarios a partir de su email
            )
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        [HttpGet(Name ="obtenerComentariosLibros")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId , [FromQuery] PaginacionDTO paginacionDTO)
        {
            var existelibro = await context.Libros.AnyAsync(libroBD => libroBD.Id == libroId);
            if (!existelibro)
            {
                return NotFound();
            }

            var queryable = context.Comentarios.Where(comentarioDB => comentarioDB.LibroId == libroId).AsQueryable(); //Con esto calculamos la cantidad de comentarios 
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var comentarios = await queryable.OrderBy(comentario => comentario.Id)
                .Paginar(paginacionDTO).ToListAsync(); //Cargamos el lsitado de comentarios relacionado con el libro ya paginado
            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }

        [HttpGet("{id:int}", Name = "obtenerComentario")] //Agregamos un nombre a la ruta para poder utuilizarla en otro metodo
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await context.Comentarios.FirstOrDefaultAsync(comentarioDB => comentarioDB.Id == id);//cones te comando obtenemos el comentario con el id
            if (comentario == null)
            {
                return NotFound();
            }
            return mapper.Map<ComentarioDTO>(comentario);//con este comando mapeamos el comentario 
        }
        [HttpPost(Name ="crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]//Con esto protegemos el endpoint y podemos acceder a sus claim
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();//Obtenemos el email del usuario a travs de sus claims
            var email = emailClaim.Value;
            var usuario = await userManager.FindByEmailAsync(email); //nos devuekve un usuario Identity
            var usuarioId = usuario.Id; //Obtenemos el Id a travez de usuario
            var existelibro = await context.Libros.AnyAsync(libroBD => libroBD.Id == libroId);
            if (!existelibro)
            {
                return NotFound();
            }
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuarioId;
            context.Add(comentario);
            await context.SaveChangesAsync();
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDTO);
        }

        [HttpPut("{id:int}",Name = "actualizarComentario")]
        public async Task<ActionResult> Put(int libroId, ComentarioCreacionDTO comentarioCreacionDTO, int id)
        {
            var existelibro = await context.Libros.AnyAsync(libroBD => libroBD.Id == libroId);
            if (!existelibro)
            {
                return NotFound();
            }
            var existeComentario = await context.Comentarios.AnyAsync(comentarioDB => comentarioDB.Id == id);
            if (!existeComentario)
            {
                return NotFound();
            }
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);//Con esto mapeamos de Comentario hacia comentarioCreacionDTO
            comentario.Id = id; //Asignamos el valor que tenemos como parametro id al id del comentario
            comentario.LibroId = id;
            context.Update(comentario); //Actualizamos el comentario
            await context.SaveChangesAsync(); //Guardamos los cambios  
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);//Mapeamos el comentario que se actualizo 
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id ,libroId = libroId},comentarioDTO); //retornamos con la ruta del comentario mapeado
        }
    }
}
