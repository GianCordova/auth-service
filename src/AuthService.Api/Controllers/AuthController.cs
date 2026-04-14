using System;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.DTOs.Email;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Obtiene la información del perfil del usuario autenticado mediante el token JWT.
    /// </summary>
    /// <returns>Retorna los datos del usuario si el token es válido.</returns>
    /// <response code="200">Retorna el perfil del usuario.</response>
    /// <response code="401">Si el usuario no está autenticado o el token es inválido.</response>
    /// <response code="404">Si el usuario no existe en la base de datos.</response>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<object>> GetProfile()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            return Unauthorized();
        }

        var user = await authService.GetUserByIdAsync(userIdClaim.Value);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(new
        {
            success = true,
            message = "Perfil obtenido exitosamente",
            data = user
        });
    }

    /// <summary>
    /// Obtiene el perfil de un usuario específico mediante su ID.
    /// </summary>
    /// <param name="request">DTO que contiene el ID del usuario a consultar.</param>
    /// <returns>Objeto con la información del usuario solicitado.</returns>
    [HttpPost("profile/by-id")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<object>> GetProfileById([FromBody] GetProfileByIdDto request)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            return BadRequest(new { success = false, message = "El userId es requerido" });
        }

        var user = await authService.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            return NotFound(new { success = false, message = "Usuario no encontrado" });
        }

        return Ok(new { success = true, message = "Perfil obtenido exitosamente", data = user });
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema con soporte para carga de archivos (avatar).
    /// </summary>
    /// <param name="registerDto">Datos de registro del usuario (incluye nombre, email, password y foto).</param>
    /// <returns>Resultado del proceso de registro.</returns>
    [HttpPost("register")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB límite
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<RegisterResponseDto>> Register([FromForm] RegisterDto registerDto)
    {
        var result = await authService.RegisterAsync(registerDto);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Inicia sesión en el sistema para obtener un token de acceso JWT.
    /// </summary>
    /// <param name="loginDto">Credenciales de acceso (Email y Password).</param>
    /// <returns>Token JWT e información básica del usuario.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);
        return Ok(result);
    }

    /// <summary>
    /// Verifica la cuenta de un usuario mediante un token enviado por correo electrónico.
    /// </summary>
    /// <param name="verifyEmailDto">Token de verificación y correo electrónico.</param>
    /// <returns>Resultado de la verificación.</returns>
    [HttpPost("verify-email")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<ActionResult<EmailResponseDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDto);
        return Ok(result);
    }

    /// <summary>
    /// Reenvía el correo electrónico con el código de verificación de cuenta.
    /// </summary>
    /// <param name="resendDto">Correo electrónico del usuario.</param>
    /// <returns>Estado del envío del correo.</returns>
    [HttpPost("resend-verification")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<EmailResponseDto>> ResendVerification([FromBody] ResendVerificationDto resendDto)
    {
        var result = await authService.ResendVerificationEmailAsync(resendDto);
        if (!result.Success)
        {
            if (result.Message.Contains("no encontrado", StringComparison.OrdinalIgnoreCase)) return NotFound(result);
            if (result.Message.Contains("ya verificado", StringComparison.OrdinalIgnoreCase)) return BadRequest(result);
            return StatusCode(503, result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Solicita un restablecimiento de contraseña enviando un token al correo del usuario.
    /// </summary>
    /// <param name="forgotPasswordDto">Correo electrónico del usuario que olvidó su clave.</param>
    /// <returns>Respuesta de confirmación del proceso.</returns>
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<EmailResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await authService.ForgotPasswordAsync(forgotPasswordDto);
        if (!result.Success) return StatusCode(503, result);
        return Ok(result);
    }

    /// <summary>
    /// Establece una nueva contraseña utilizando el token de restablecimiento recibido.
    /// </summary>
    /// <param name="resetPasswordDto">Token, nueva contraseña y confirmación.</param>
    /// <returns>Resultado del cambio de contraseña.</returns>
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<EmailResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDto);
        return Ok(result);
    }
}