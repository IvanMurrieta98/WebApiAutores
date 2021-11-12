using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebAPIAutores.Filtros;
using WebAPIAutores.Middleware;
using WebAPIAutores.Servicios;
using WebAPIAutores.Utilidades;


[assembly: ApiConventionType(typeof(DefaultApiConventions))]//Esto nos sirve para visualizar las posibles respuestas en toda la aplicacion
namespace WebAPIAutores 
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); //Con esto limpiamos el mapeo que existe de los claims 
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllers(
                opciones => { opciones.Filters.Add(typeof(FiltroDeExcepcion));//Con esto agregamos un filtro de maneraa global
                    opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
                }).
                AddNewtonsoftJson(options =>options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);//este comando es para ingnorar las solicitudes ciclicas
            
            //Con este comando creamos el servicio para acceder a la BD
            services.AddDbContext<AplicationDBContext>(options => 
            options.UseSqlServer(Configuration.GetConnectionString("defaultConnetions")));

            //services.AddTransient<IServicio, ServicioA>();
            // addTransient se genera un servicio del tipo transitorio, siempre se va a dar una nueva instancia
            //addScoped siempre te va a dar la misma instancia en el pero cambia entre contextos http
            //addSingleton siempre se va a dar la misma instancia incliuso en distintas peticiones http 
            //services.AddTransient<ServicioTransient>();
            //services.AddScoped<ServicioScoped>();
            //services.AddSingleton<ServicioSingleton>();
            //services.AddTransient<MiFiltroDeAccion>();
            //services.AddHostedService<EscribirEnArchivo>();

            //services.AddResponseCaching(); //servicio de filtro de memoria cache

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters { 
                ValidateIssuer = false,//no validamos el ISSUER
                ValidateAudience = false,//tampoco la udiencia
                ValidateLifetime= true,//VAlidamos el tiempo de vida
                ValidateIssuerSigningKey = true,//validamos la firma
                IssuerSigningKey= new SymmetricSecurityKey(     //Configuramos la firma 
                    Encoding.UTF8.GetBytes(Configuration["llaveJWT"])),
                ClockSkew= TimeSpan.Zero
                });    // filtro de autorizacion

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { 
                    Title = "WebAPIAutores", 
                    Version = "v1",
                    Description = "Este es un web api para trabajar con autores y libros",
                    Contact= new OpenApiContact
                    { 
                        Email = "ivan@example.com",
                        Name = "Ivan Murrieta",
                        Url = new Uri("https://web.facebook.com/ITMS24")
                    }
                });
                c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });//agregamos el nuevo documento de swagger para la v2
                c.OperationFilter<AgregarParametroHATEOAS>();//agrega el parametro a los enpoints
                c.OperationFilter<AgregarParametrosXVersion>();

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{ }
                    }
                });

                //esto espara visualizar comentarios a nivel de nuestros endpoints
                var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
                c.IncludeXmlComments(rutaXML);
            });

            services.AddAutoMapper(typeof(Startup));//configura el automapper 
            
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AplicationDBContext>()
                .AddDefaultTokenProviders();

            //Vamos a confiurar un jwt de administrador basada en claims 
            services.AddAuthorization(opciones =>
            {
                opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("esAdmin"));//agregamos una politica de seguridad
                //podemos usar diferentes politicas para que accedan a diferntes partes del sistema 
            });

            //Activamos los servicios de proteccion de datos 
            services.AddDataProtection();

            //Activamos el servicio de hash
            services.AddTransient<HashService>();

            //Configuramos el servicio de CORS para que puedan acceder al web api desde cualquier parte 
            services.AddCors(opciones =>
            {
                opciones.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("https://www.apirequest.io")     //URLs que van a poder tener acceso anuestro web api
                    .AllowAnyMethod()           //Permite cualquier metodo(get,post,put,delete)
                    .AllowAnyHeader()           //Permite cualquier cabecera
                    .WithExposedHeaders(new string[] { "cantidadTotalRegistros" }); //Con esto damos los permisos para leer las cabeceras personalizadas
                });
            });

            services.AddTransient<GeneradorEnlaces>();
            services.AddTransient<HATEOASAutorFilterAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectionString"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)//con sistema de inyeccion de dependencias mandamos a traer la clase Ilogger
        {

            //Middleware

            //Middleware que guarde todos los registros antes de ser enviados a nuestros clientes

            //app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();//Mandamos a traer el middelware escribiendo la clase utiilizada
            app.UseLoguearRespuestaHTTP();//esta linea funciona igual que la de arriba pero sin exponer la clase utilizada


            //Esto lo movi a la clase UseLoguearRespuestaHTTP
            //app.Use(async (contexto, siguiente) =>{
            //using (var ms = new MemoryStream())
            //{
            //    var cuerpoOriginalRespuesta = contexto.Response.Body;
            //    contexto.Response.Body = ms;

            //    await siguiente.Invoke(); //esta instruccion nos permite continuar 

            //    ms.Seek(0, SeekOrigin.Begin);
            //    string respuesta = new StreamReader(ms).ReadToEnd(); //esta funcion va a guardar la respuesta que obtenga el cliente en la memory stream
            //    ms.Seek(0, SeekOrigin.Begin);

            //    await ms.CopyToAsync(cuerpoOriginalRespuesta);
            //    contexto.Response.Body = cuerpoOriginalRespuesta;
            //    logger.LogInformation(respuesta);
            //}
            //});

            //app.Map("/ruta1", app =>
            //{
            //    app.Run(async contexto =>
            //    {
            //        await contexto.Response.WriteAsync("Estoy interseptado la tubería");
            //    });
            //});

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            //app.UseResponseCaching(); //filtro de memoria cache
            app.UseCors();       //con esto tenemos cors activados en el web api

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
