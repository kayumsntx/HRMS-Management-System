using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Same /api/idcards prefix as IdCardsController; different sub-routes.
[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/idcards")]
public class IdCardFilesController : ControllerBase
{
    private readonly IIdCardDocumentService _service;

    public IdCardFilesController(IIdCardDocumentService service)
    {
        _service = service;
    }

    // GET: api/idcards/1/pdf   (front + back, CR80 size). Printing is counted separately by POST /api/idcards/{id}/print.
    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> Pdf(int id)
    {
        var file = await _service.GetPdfAsync(id);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("{id:int}/qr")]
    public async Task<IActionResult> Qr(int id)
    {
        var file = await _service.GetQrAsync(id);
        return File(file.Content, file.ContentType);
    }
}
