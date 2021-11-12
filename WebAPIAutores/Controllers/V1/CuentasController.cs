using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebAPIAutores.DTOs;
using WebAPIAutores.Servicios;

namespace WebAPIAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager, //uso de roles de administracion
            IDataProtectionProvider dataProtectionProvider,//Encriptacion
            HashService hashService //hash 
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            dataProtector =dataProtectionProvider.CreateProtector("valor_unico_y_quizas_secreto");//encritacion
        }

        //[HttpGet("hash/{textoPlano}")]
        //public ActionResult Encriptar(string textoPlano)
        //{
        //    var resultado1 = hashService.Hash(textoPlano);
        //    var resultado2 = hashService.Hash(textoPlano);

        //    return Ok(new {
        //        textoPlano = textoPlano,
        //        Hash1 = resultado1,
        //        Hash2 = resultado2
        //    });
        //}


        //[HttpGet("encriptar")]
        //public ActionResult Encriptar()
        //{
        //    var textoPlano = "Ivan Murrieta";
        //    var textoCifrado = dataProtector.Protect(textoPlano);
        //    var textoDesencriptado = dataProtector.Unprotect(textoCifrado);

        //    return Ok(new
        //    {
        //        textoPlano = textoPlano,
        //        textoCifrado = textoCifrado,
        //        textoDesencriptado = textoDesencriptado
        //    });
        //}

        //[HttpGet("encriptarPorTiempo")]
        //public ActionResult EncriptarPorTiempo()
        //{
        //    var protectorLimitadoPorTiempo = dataProtector.ToTimeLimitedDataProtector();

        //    var textoPlano = "Ivan Murrieta";
        //    var textoCifrado = protectorLimitadoPorTiempo.Protect(textoPlano,lifetime: TimeSpan.FromSeconds(5));
        //    Thread.Sleep(6000);
        //    var textoDesencriptado = protectorLimitadoPorTiempo.Unprotect(textoCifrado);

        //    return Ok(new
        //    {
        //        textoPlano = textoPlano,
        //        textoCifrado = textoCifrado,
        //        textoDesencriptado = textoDesencriptado
        //    });
        //}



        [HttpPost("registrar",Name ="registrarUsuario")]//api/cuentas/registrar
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuario credencialesUsuario) 
        {
            var usuario = new IdentityUser { UserName = credencialesUsuario.Email, Email = credencialesUsuario.Email }; //mandamos a traer el email
            var resultado = await userManager.CreateAsync(usuario, credencialesUsuario.Password); //creamos al usuario
            if (resultado.Succeeded)
            {
                //aqui vamos a agregar el token (JWT) para poder acceder 
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        //Creamos el endpoint de login
        [HttpPost("login",Name = "LoginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuario credencialesUsuario)
        {
            //Para crear el login necesitamos utilizar un clase llamada SignInManager
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuario.Email,
                credencialesUsuario. Password,isPersistent:false, lockoutOnFailure:false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return BadRequest("Login Incorrecto");
            }
        }

        [HttpGet("RenovarToken",Name ="renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();//Obtenemos el email del usuario a travs de sus claims
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuario()
            {
                Email = email
            };
            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<RespuestaAutenticacion> ConstruirToken(CredencialesUsuario credencialesUsuario)
        {
            //creamos un listado de claims(es informacion del usuario en la cual podemos confiar)
            var claims = new List<Claim>()
            {
                new Claim("email",credencialesUsuario.Email),//los claims es un par de llave acompañado de su valor
                new Claim("lo que yo quiera","cualquier otro valor")
                //Es importante saber que los claims tambien se pueden ver del lado del cliente por eso no hay que poner informacion confidencial del usuario en los claims
            };

            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);//Buscamos al usuario por su email
            var claimsDB = await userManager.GetClaimsAsync(usuario);//obtenermos los claims del usuario

            claims.AddRange(claimsDB);
            //vamos a crear el JWT
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        //aqui va la llave secreta la cual pondremos en un provedor de configuracion 
                    configuration["llaveJWT"]
                ));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);//Creamos las credenciales

            var expiracion = DateTime.UtcNow.AddDays(30); //ponemos expiracion al token 

            //Ahora construimos el token
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);
            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        [HttpPost("HacerAdmin",Name ="hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO) //creamos un DTO(EditarAdminDTO) para acceder al email del usuario
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);//Buscamos al usuario por su email
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1")); //Agregamos un nuevo claim al usuario que encontramos 
            return NoContent();
        }

        [HttpPost("RemoverAdmin",Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO) //creamos un DTO(EditarAdminDTO) para acceder al email del usuario
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);//Buscamos al usuario por su email
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1")); //Agregamos un nuevo claim al usuario que encontramos 
            return NoContent();
        }

    }
}
