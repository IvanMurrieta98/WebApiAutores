using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPIAutores.Entidades;

namespace WebAPIAutores
{
    public class AplicationDBContext : IdentityDbContext
    {
        public AplicationDBContext( DbContextOptions options) : base(options)
        {
        }
        // Para crear las tablas en la consola de administrador de paquetes se escribe el comando Add-Migration nombre de la tabla
        //despues actualizamos la base de datos con Update-Database

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AutorLibro>()
                .HasKey(al => new { al.AutorID, al.LibroId });// con esto creamos una llave primaria que hace referancia al id dela autor y del libro 
        }
        public DbSet<Autor> Autores { get; set; }//nos permite realizar querys directamente a la tabla
        public DbSet<Libro> Libros { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }




    }
}
