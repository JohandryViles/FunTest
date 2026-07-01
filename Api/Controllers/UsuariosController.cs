using Api.Application.Features.Usuarios.Commands;
using Api.Application.Features.Usuarios.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginatedUsuariosDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedUsuariosDto>> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default
    )
    {
        var usuarios = await sender.Send(new GetUsuariosQuery(page, pageSize), cancellationToken);
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var usuario = await sender.Send(new GetUsuarioByIdQuery(id), cancellationToken);

        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Create(
        [FromBody] CreateUsuarioCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var usuario = await sender.Send(command, cancellationToken);
        return Ok(usuario);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> Update(
        Guid id,
        [FromBody] UpdateUsuarioRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var usuario = await sender.Send(
            new UpdateUsuarioCommand(id, request.Nombre, request.Apellido, request.Email),
            cancellationToken
        );

        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await sender.Send(new DeleteUsuarioCommand(id), cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
