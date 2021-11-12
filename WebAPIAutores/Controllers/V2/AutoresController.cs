using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;
using WebAPIAutores.Filtros;
using WebAPIAutores.Servicios;
using WebAPIAutores.Utilidades;

namespace WebAPIAutores.Controllers.V2
{
    [ApiController]
    [Route("api/autores")]// api/autores => ruta
    [CabeceraEstaPresente("x-version", "2")]
    //[Route("api/v2/autores")]// api/autores => ruta
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme  //Con esto restringimos el metodo para que solo puedan acceder usuarios autorizados
        ,Policy ="EsAdmin")] //agregamos la politia en la autorizacion
    public class AutoresController : ControllerBase
    {
        private readonly AplicationDBContext context;
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly IAuthorizationService authorizationService;

        //private readonly IServicio servicio;
        //private readonly ServicioTransient servicioTransient;
        //private readonly ServicioScoped servicioScoped;
        //private readonly ServicioSingleton servicioSingleton;
        //private readonly ILogger<AutoresController> logger;

        public AutoresController(AplicationDBContext context , IMapper mapper, //Mandamos a traer el servicio de mapper
            IConfiguration configuration //con esto mandamos a traer las configuraciones de microsoft 
            ,IAuthorizationService authorizationService
            //, IServicio servicio, 
            //ServicioTransient servicioTransient, ServicioScoped servicioScoped, 
            //ServicioSingleton servicioSingleton, ILogger<AutoresController> logger
            ) // Ilogger sirve para mandar mensajes log 
        {
            this.context = context;
            this.mapper = mapper;
            this.configuration = configuration;
            this.authorizationService = authorizationService;
            //this.servicio = servicio;
            //this.servicioTransient = servicioTransient;
            //this.servicioScoped = servicioScoped;
            //this.servicioSingleton = servicioSingleton;
            //this.logger = logger;
        }

        //[HttpGet("GUID")]
        //[ResponseCache(Duration = 10)]//esto hace que la respuesta se guarde en cache y sea igual durante los proximos 10 segundos
        //[ServiceFilter(typeof(MiFiltroDeAccion))]
        //public ActionResult ObtenerGuids()
        //{
        //    return Ok
        //        (new {
        //            AutoresController_Transient = servicioTransient.Guid,
        //            ServicioA_Transient = servicio.ObtenerTransient(),
        //            AutoresController_Scoped = servicioScoped.Guid,
        //            ServicioA_Scoped = servicio.ObtenerScoped(),
        //            AutoresController_Singleton = servicioSingleton.Guid,
        //            ServicioA_Singleton = servicio.ObtenerSingleton()
        //        });
        //}

        //obtencion de datos ruta=>api/autores
        //[HttpGet("listado")]//api/autores/listado
        //[HttpGet("/listado")]ruta=>/listado, fuera de api/autores
        //[ResponseCache(Duration = 10)]//esto hace que la respuesta se guarde en cache y sea igual durante los proximos 10 segundos
        //[Authorize]//filtro de autorizacion
        //[ServiceFilter(typeof(MiFiltroDeAccion))]//manda atraer el servicio de filtro

        //[HttpGet("configuraciones")]
        //public ActionResult<string> ObtenerConfiguracion()
        //{
        //    return configuration["apellido"];//con esto mandamos a traer el campo del cual queremos obtener su valor
        //}

        [HttpGet(Name = "obtenerAutoresv2")]
        [AllowAnonymous]//Esto permite que puedan acceder usuarios anonimos
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<List<AutorDTO>>> Get() //incuimos el booleano por si queremos enlaces de HATEOAS
        {
            //throw new NotImplementedException();

            //logger.LogInformation("Estamos obteniendo los autores");
            //logger.LogWarning("Este es un mensaje de prueba");
            //servicio.RealizarTarea();
            //return await context.Autores.ToListAsync(); //retorna la lista de autores de manera asincrona
            var autores = await context.Autores.ToListAsync();
            autores.ForEach(autor => autor.Nombre = autor.Nombre.ToUpper());
            return mapper.Map<List<AutorDTO>>(autores); //mandamos la lista de AutorDTO a la lista autores
            //if (incluirHATEOAS)
            //{
            //    var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");//Agregamoes el authorization para ver los enlaces de adminstrador
            //    //dtos.ForEach(dto => GenerarEnlaces(dto, esAdmin.Succeeded)); //generamos los enlaces de los dtos, y si es admin mostramos los enlaces de administrador
                
            //    var resultado = new ColeccionDeRecursos<AutorDTO> { Valores = dtos };
            //    resultado.Enlaces.Add(new DatoHATEOAS(
            //        enlace: Url.Link("obtenerAutores", new { }),
            //        descripcion: "self",
            //        metodo: "GET"));

            //    if (esAdmin.Succeeded)
            //    {
            //        resultado.Enlaces.Add(new DatoHATEOAS(
            //        enlace: Url.Link("crearAutor", new { }),
            //        descripcion: "crear-Autor",
            //        metodo: "POST"));
            //    }
            //    return Ok(resultado);
            //}
            
            //return Ok(dtos);
        }

        //[HttpGet("primero")] //api/autores/primero
        //public async Task<ActionResult<Autor>> PrimerAutor()
        //{
        //    return await context.Autores.FirstOrDefaultAsync();
        //}

        [HttpGet("{id:int}", Name = ("obtenerAutorv2"))]// obtencion a travez de una variable que se coloca entre llaves{}la restriccion es :int,
                                                      // Agregamos un nombre a la ruta para acceder a ella atraves del nombre 
        [AllowAnonymous]
        [ServiceFilter(typeof(HATEOASAutorFilterAttribute))]
        public async Task<ActionResult<AutorDTOConLibros>> Get(int id)
        {
           var autor= await context.Autores
                .Include(autorDB => autorDB.AutoresLibros)//Incluimos la clase autores libros
                .ThenInclude(autorLibroDB => autorLibroDB.Libro)//Accedemos a la clase libros que esta incluida en la clase autoreslibros
                .FirstOrDefaultAsync(autorBD => autorBD.Id == id); //Buscamos el autor por id
            if (autor == null)
            {
                return NotFound();
            }

            var dto= mapper.Map<AutorDTOConLibros>(autor);
            //var esAdmin = await authorizationService.AuthorizeAsync(User, "esAdmin");//Agregamoes el authorization para ver los enlaces de adminstrador
            //GenerarEnlaces(dto,esAdmin.Succeeded);//generamos los enlaces de los dtos, y si es admin mostramos los enlaces de administrador
            return dto;

            
        }

        
        //movemos esta seccion de codigo al servicio que generara los enlaces 
        //private void GenerarEnlaces(AutorDTO autorDTO, bool esAdmin) //agregamoe el autordto para ver los enlaces, ademas del valor si es admin
        //{
        //    autorDTO.Enlaces.Add(new DatoHATEOAS(
        //        enlace: Url.Link("obtenerAutor", new { id = autorDTO.Id }),
        //        descripcion: "Self", 
        //        metodo: "GET"));

        //    if (esAdmin)
        //    {
        //        autorDTO.Enlaces.Add(new DatoHATEOAS(
        //        enlace: Url.Link("actualizarAutor", new { id = autorDTO.Id }),
        //        descripcion: "autor-actualizar",
        //        metodo: "PUT"));

        //        autorDTO.Enlaces.Add(new DatoHATEOAS(
        //            enlace: Url.Link("borrarAutor", new { id = autorDTO.Id }),
        //            descripcion: "autor-borrar",
        //            metodo: "DELETE")); 
        //    }

            
        //}

        [HttpGet("{Nombre}",Name =("obtenerAutorPorNombrev2"))]// obtencion a travez de una variable que se coloca entre llaves{} 
        public async Task<ActionResult<List<AutorDTO>>> GetPorNombre(string nombre)
        {
            var autores = await context.Autores.Where(autorBD => autorBD.Nombre.Contains(nombre)).ToListAsync(); ;//query que contenga el nombre a travez de una lista


            return mapper.Map<List<AutorDTO>>(autores);
        }


        [HttpPost(Name = "crearAutorv2")]
        public async Task<ActionResult> Post([FromBody] AutorCreacionDTO autorCreacionDTO)
        {
            var ExisteAutorConeElMismoNombre = await context.Autores.AnyAsync(x => x.Nombre == autorCreacionDTO.Nombre);//verifica que no exista un autor con el mismo nombre en la BD
            if (ExisteAutorConeElMismoNombre )
            {
                return BadRequest($"Ya existe un autor con el nombre {autorCreacionDTO.Nombre}");
            }
            var autor = mapper.Map<Autor>(autorCreacionDTO);//con este comando mandamos a traer los campos que tiene autorCreacionDTO a la clase autor

            context.Add(autor);//agrega el autor en el control
            await context.SaveChangesAsync();//guarda los cambios de manera asincrona

            var autorDTO = mapper.Map<AutorDTO>(autor);//Mapeamos el autor a travez de AutorDTO 

            return CreatedAtRoute("obtenerAutorv2", new { id = autor.Id},autorDTO);//retornamos la ruta de un recurso ya creado
        }

        [HttpPut("{id:int}",Name = "actualizarAutorv2")]//para acturalizar un autor, se combina la ruta del controlador (Route) + el id 
        public async Task<ActionResult> Put(AutorCreacionDTO autorCreacionDTO, int id) 
        {
            
            var existe = await context.Autores.AnyAsync(x => x.Id == id); //AnyAsyn nos sirve para si ver si existe el objeto
            if (!existe)
            {
                return NotFound(); // nos devuelve un Error 404
            }
            var autor = mapper.Map<Autor>(autorCreacionDTO);
            autor.Id = id;
            context.Update(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutorv2", new {id = autor.Id },autorDTO);
        }

        /// <summary>
        /// Borra un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar</param>
        /// <returns></returns>
        [HttpDelete("{id:int}",Name = "borrarAutorv2")]//para eliminar un autor, se combina la ruta del controlador (Route) + el id (api/autores/id)
        public async Task<ActionResult> Delete(int id) 
        {
            var existe = await context.Autores.AnyAsync(x=> x.Id == id); //AnyAsyn nos sirve para si ver si existe el objeto
            if (!existe) 
            {
                return NotFound(); // nos devuelve un Error 404
            }
            context.Remove(new Autor() { Id = id }); //marcamos al autor con remove para despues eliminarlo 
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
