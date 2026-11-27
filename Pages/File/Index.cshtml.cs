using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Pages.File;

public class IndexModel : AppPageModel
{
    private readonly AppFileService service;
    public IndexModel(AppFileService service) => this.service = service;
    [BindProperty(SupportsGet = true)] public string? Path { get; set; }
    public string Current => Path ?? "";
    public string Parent => AppFileService.Parent(Current);
    public List<AppFileService.Item> Items { get; private set; } = new();
    [TempData] public string? Message { get; set; }
    [TempData] public bool Failed { get; set; }
    public IActionResult OnGet()
    {
        try { Items = service.List(Current); return Page(); }
        catch { return BadRequest("Invalid or inaccessible folder."); }
    }
    private async Task<IActionResult> Run(Func<Task> action)
    {
        try { service.List(Current); await action(); Message = "Operation completed successfully."; Failed = false; }
        catch (InvalidOperationException ex) { Message = ex.Message; Failed = true; }
        catch (Exception) { Message = "Operation failed. Check paths, permissions and available storage. Some uploaded files may have been saved."; Failed = true; }
        return RedirectToPage("./Index", new { path = Current });
    }
    private Task<IActionResult> RunSync(Action action) => Run(() => { action(); return Task.CompletedTask; });
    public Task<IActionResult> OnPostCreateFolder(string? name) => RunSync(() => service.CreateFolder(Current, name ?? ""));
    public Task<IActionResult> OnPostUploadFileAsync(List<IFormFile> files) => Run(() => service.UploadAsync(Current, files));
    public Task<IActionResult> OnPostUploadFolderAsync(List<IFormFile> files) => Run(() => service.UploadAsync(Current, files, true));
    public Task<IActionResult> OnPostDelete(string? target) => RunSync(() => service.Delete(target ?? ""));
    public Task<IActionResult> OnPostRename(string? target, string? name) => RunSync(() => service.Rename(target ?? "", name ?? ""));
    public Task<IActionResult> OnPostMove(string? target, string? destination) => RunSync(() => service.Move(target ?? "", destination ?? ""));
}
