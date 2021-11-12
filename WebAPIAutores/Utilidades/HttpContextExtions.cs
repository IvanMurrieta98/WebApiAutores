using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPIAutores.Utilidades
{
    public static class HttpContextExtions
    {
        public async static Task InsertarParametrosPaginacionEnCabecera<T>(this HttpContext httpContext, IQueryable<T> queryable)
        {
            if (httpContext == null )
            {
                throw new ArgumentNullException(nameof(httpContext));

            }
            
            var cantidad = await queryable.CountAsync();//Esto lo utilizamos para contar los registros de la tabla que obtenemos a travez de IQueryable
            httpContext.Response.Headers.Add("cantidadTotalRegistros", cantidad.ToString());//esto lo utilizamos para colocar en la cabecera de la respuesta el dato de cantidad total de registros a mostrar 


        }
    }
}
