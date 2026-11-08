using L1CacheSystem;
using L1CacheSystem.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsageShowcaseService.Dtos;
using UsageShowcaseService.Services.Interfaces;

namespace UsageShowcaseService.Controllers;

[Route("[controller]")]
[ApiController]
public class ScreamController : ControllerBase
{
    private readonly ILogger<ScreamController> _logger;
    private readonly IServiceCacheSystem _cacheSystem;

    public ScreamController(
        IServiceCacheSystem cacheSystem,
        ILogger<ScreamController> logger)
    {
        _logger = logger;
        _cacheSystem = cacheSystem;
    }

    /// <summary>
    /// Bring back all the echoes that are still sounding
    /// </summary>
    /// <param name="wordsRemainNotLess">How many words are left for the echo to utter?</param>
    [HttpGet("echos/{wordsRemainNotLess:int}")]
    [ProducesResponseType(typeof(EchoDto[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEchos([FromRoute] int wordsRemainNotLess)
    {
        var echos = _cacheSystem.GetAllEcho(wh => wh.ResidualSound?.Length >= wordsRemainNotLess, returnType: ReturnType.Reference);
        var dtos = echos.Select( value => new EchoDto{ ResidualSound = value.ResidualSound }).ToArray();

        return Ok(dtos);
    }

    [HttpPost("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ToScream([FromBody] ScreamDto screamDto, CancellationToken ct)
    {
        var newScreamId = ScreamValue.GetNextId();

        await using var lockGuard = new LockGuard(_logger, null);
        await _cacheSystem.GetScreamLockAsync(newScreamId, lockGuard, ct);
        await _cacheSystem.GetEchoLockAsync(newScreamId, lockGuard, ct);

        _cacheSystem.SetScream(
            new ScreamValue
            {
                Id = newScreamId,
                Sound = screamDto.Sound
            },
            ValueOwnership.Transferred);

        _cacheSystem.SetEcho(
            new Values.EchoValue
            {
                Id = newScreamId,
                ResidualSound = screamDto.Sound
            },
            ValueOwnership.Transferred);

        return Ok();
    }
}