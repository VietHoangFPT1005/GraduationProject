using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly MyApiService _myApiService;

    public MyController(MyApiService myApiService)
    {
        _myApiService = myApiService;
    }

    [HttpGet("get-data")]
    public async Task<IActionResult> GetData()
    {
        var data = await _myApiService.GetDataAsync();
        return Ok(data);
    }
}
