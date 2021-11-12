using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.Middleware
{

    //crear una clase estatica para utilizar un metodo de extencion y solo se pueden colocar en clases estaticas 
    public static class LoguearRespuestaHTTPMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoguearRespuestaHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
        }
    }
    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMiddleware> logger;

        //requestdelegate significa que vamos a mandar a traer los siguientes middleware de la tuberia
        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente, ILogger<LoguearRespuestaHTTPMiddleware>logger )
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }

        //una de las reglas para poder utilizar esta clase como middleware necesita llevar forsozamente un metodo publico invoke o invokeasync
        //este metodo debe retornar una tarea y aceptar como primer parametro un Http Context
        public async Task InvokeAsync(HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginalRespuesta = contexto.Response.Body;
                contexto.Response.Body = ms;

                await siguiente(contexto); //esta instruccion nos permite continuar y mandamos a traer el delegado 

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd(); //esta funcion va a guardar la respuesta que obtenga el cliente en la memory stream
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalRespuesta);
                contexto.Response.Body = cuerpoOriginalRespuesta;
                logger.LogInformation(respuesta);
            }
        }
    }
}
