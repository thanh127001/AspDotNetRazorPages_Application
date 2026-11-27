using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services;

public class AppFileService
{
    private readonly string root;
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico", ".pdf", ".txt", ".csv", ".docx", ".xlsx", ".pptx", ".zip", ".html", ".js", ".css", ".json", ".xml", ".md" };
    public AppFileService(IWebHostEnvironment env)
    {
        var data = System.IO.Path.Combine(env.ContentRootPath, "AppData");
        Directory.CreateDirectory(data);
        if (Link(data)) throw new InvalidOperationException("Invalid data directory.");
        root = System.IO.Path.GetFullPath(System.IO.Path.Combine(data, "Files"));
        Directory.CreateDirectory(root);
        if (Link(root)) throw new InvalidOperationException("Invalid root.");
    }
    public record Item(string Name, string Path, bool Folder, bool Protected, long Size);
    public static string Parent(string path) => path.Contains('/') ? path[..path.LastIndexOf('/')] : "";
    private static string Join(string a, string b) => string.IsNullOrEmpty(a) ? b : a + "/" + b;
    private static bool Link(string p) => (System.IO.File.GetAttributes(p) & FileAttributes.ReparsePoint) != 0;
    private string Resolve(string path)
    {
        var current = root;
        if (Link(root)) throw new InvalidOperationException("Invalid root.");
        if (string.IsNullOrEmpty(path)) return current;
        var parts = path.Split('/');
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part) || part is "." or ".." || part.EndsWith('.') || part.EndsWith(' ') || part.IndexOfAny(new[] {'\\', ':', '*', '?', '"', '<', '>', '|'}) >= 0 || part.StartsWith('.') || System.Text.RegularExpressions.Regex.IsMatch(part, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase) || part.Any(char.IsControl) || part.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException("Invalid path.");
            current = System.IO.Path.Combine(current, part);
            // GetAttributes also detects dangling symbolic links on supported platforms.
            try { if (Link(current)) throw new InvalidOperationException("Links are not allowed."); }
            catch (FileNotFoundException) { } catch (DirectoryNotFoundException) { }
        }
        return current;
    }
    public List<Item> List(string path)
    {
        var dir = Resolve(path);
        return new DirectoryInfo(dir).EnumerateFileSystemInfos().Select(x => new Item(x.Name, Join(path,x.Name), (x.Attributes & FileAttributes.Directory) != 0, (x.Attributes & FileAttributes.ReparsePoint) != 0, x is FileInfo f && (x.Attributes & FileAttributes.ReparsePoint) == 0 ? f.Length : 0)).OrderByDescending(x=>x.Folder).ThenBy(x=>x.Name).ToList();
    }
    public Item Get(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new InvalidOperationException("Root cannot be deleted.");
        var p = Resolve(path);
        var folder = Directory.Exists(p);
        if (!folder && !System.IO.File.Exists(p)) throw new FileNotFoundException();
        return new Item(System.IO.Path.GetFileName(p),path,folder,false,folder?0:new FileInfo(p).Length);
    }
    public async Task UploadAsync(string path, List<IFormFile> files, bool folder = false)
    {
        if (!Directory.Exists(Resolve(path))) throw new DirectoryNotFoundException();
        if (files.Count > 10000 || files.Sum(f => f.Length) > 512L * 1024 * 1024) throw new InvalidOperationException("Upload limit exceeded.");
        if (files.Count == 0) throw new InvalidOperationException("Select files first.");
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var plan = new List<(IFormFile File,string Target)>();
        foreach(var file in files)
        {
            var name = file.FileName.Replace('\\','/');
            if (!folder) name = name.Split('/').Last();
            else if (!name.Contains('/')) throw new InvalidOperationException("Missing relative folder path.");
            if (!Allowed.Contains(System.IO.Path.GetExtension(name))) throw new InvalidOperationException("File type not allowed.");
            var target = Resolve(Join(path,name));
            if (!targets.Add(target) || System.IO.File.Exists(target) || Directory.Exists(target)) throw new InvalidOperationException("An item already exists.");
            plan.Add((file,target));
        }
        foreach(var entry in plan)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(entry.Target)!);
            using var stream = new FileStream(entry.Target, FileMode.CreateNew);
            await entry.File.CopyToAsync(stream);
        }
    }
    public void CreateFolder(string path, string name)
    {
        if (!Directory.Exists(Resolve(path))) throw new DirectoryNotFoundException();
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/')) throw new InvalidOperationException("Invalid folder name.");
        var target = Resolve(Join(path, name));
        if (Directory.Exists(target) || System.IO.File.Exists(target)) throw new InvalidOperationException("An item already exists.");
        Directory.CreateDirectory(target);
    }
    public void Rename(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Contains('/')) throw new InvalidOperationException("Invalid name.");
        Relocate(path, Join(Parent(path), name));
    }
    public void Move(string path, string destination)
    {
        if (!Directory.Exists(Resolve(destination))) throw new InvalidOperationException("Destination folder does not exist.");
        Relocate(path, Join(destination, Get(path).Name));
    }
    private void Relocate(string path, string destination)
    {
        var item = Get(path);
        var source = Resolve(path);
        var target = Resolve(destination);
        if (!item.Folder && !Allowed.Contains(System.IO.Path.GetExtension(target))) throw new InvalidOperationException("File type not allowed.");
        if (target.Equals(source, StringComparison.OrdinalIgnoreCase) || target.StartsWith(source + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Invalid destination.");
        if (Directory.Exists(target) || System.IO.File.Exists(target)) throw new InvalidOperationException("An item already exists.");
        if (item.Folder) { CheckTree(source); Directory.Move(source, target); }
        else System.IO.File.Move(source, target);
    }
    private static void CheckTree(string path)
    {
        foreach(var item in Directory.EnumerateFileSystemEntries(path))
        {
            if (Link(item)) throw new InvalidOperationException("Folder contains a link.");
            if (Directory.Exists(item)) CheckTree(item);
        }
    }
    public void Delete(string path)
    {
        var item = Get(path); var p = Resolve(path);
        if (item.Folder) { CheckTree(p); Directory.Delete(p,true); }
        else System.IO.File.Delete(p);
    }
}
