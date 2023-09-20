using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;

namespace Translating.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ITranslationRepository _translationsRepository;

    public HomeController(ITranslationRepository translationRepository)
    {
        _translationsRepository = translationRepository;
    }

    public async Task<IActionResult> Index()
    {
        //var user = User.Claims;

        //var accessToken = await HttpContext.GetTokenAsync(Tokens.BackchannelAccessToken);

        var model = await _translationsRepository.GetAllAsync();

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var model = new CreateTranslationModel();
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTranslationModel translation)
    {
        var createdModel = await _translationsRepository.AddAsync(translation);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _translationsRepository.GetByIdAsync(id);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TranslationModel translation)
    {
        await _translationsRepository.UpdateAsync(translation);

        return RedirectToAction("Index");
    }

}
