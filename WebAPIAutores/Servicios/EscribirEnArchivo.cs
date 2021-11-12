using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPIAutores.Servicios
{
    public class EscribirEnArchivo : IHostedService
    {
        private readonly IWebHostEnvironment env;
        private readonly String NombreArchivo = "Archivo 1.txt";
        private Timer timer;

        public EscribirEnArchivo(IWebHostEnvironment env )
        {
            this.env = env;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(Dowork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            Escribir("Proceso iniciado");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            Escribir("Proceso finalizado");
            return Task.CompletedTask;
        }

        private void Dowork(object state)
        {
            Escribir("Proceso en ejecución:"+ DateTime.Now.ToString("dd/mm/yyyy hh:mm:ss"));//metodo para escribir cada 5 segundos en el archivo de texto 
        }

        private void Escribir(string mensaje)
        {

            var ruta = $@"{env.ContentRootPath}\wwwroot\{NombreArchivo}";
            using (StreamWriter writer = new StreamWriter(ruta, append: true))
            {
                writer.WriteLine(mensaje);
            }
        }
    }
}
