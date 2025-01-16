using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using CvMaker.Models;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace CvMaker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPdf([FromForm] string name, [FromForm] string surname, 
        [FromForm] string email, [FromForm] string phone, [FromForm] string address,
        [FromForm] string skills, [FromForm] string school, [FromForm] string department, 
        [FromForm] string aboutText, [FromForm] DateTime startDate, [FromForm] DateTime finishDate,
        [FromForm] IFormFile photo)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        using var gfx = XGraphics.FromPdfPage(page);

        // Colors
        var burgundy = XColor.FromArgb(255, 146, 84, 89);
        var white = XColors.White;
        
        // Fonts
        var titleFont = new XFont("Arial", 32, XFontStyle.Bold);
        var subtitleFont = new XFont("Arial", 18);
        var headingFont = new XFont("Arial", 16, XFontStyle.Bold);
        var normalFont = new XFont("Arial", 12);

        // Draw header background
        var headerBrush = new XSolidBrush(burgundy);
        gfx.DrawRectangle(headerBrush, 0, 0, page.Width, 120);

        // Draw profile image or circle
        double circleX = 100;
        double circleY = 100;
        double circleRadius = 80;

        try
        {
            if (photo != null && photo.Length > 0)
            {
                using var stream = new MemoryStream();
                await photo.CopyToAsync(stream);
                stream.Position = 0;
                
                // Create image from stream
                using var image = XImage.FromStream(stream);

                // Draw the image in a circle
                var state = gfx.Save();
                var path = new XGraphicsPath();
                path.AddEllipse(circleX - circleRadius, circleY - circleRadius, circleRadius * 2, circleRadius * 2);
                gfx.IntersectClip(path);
                gfx.DrawImage(image, circleX - circleRadius, circleY - circleRadius, circleRadius * 2, circleRadius * 2);
                gfx.Restore(state);
            }
            else
            {
                gfx.DrawEllipse(new XSolidBrush(XColors.LightGray), 
                    circleX - circleRadius, circleY - circleRadius, 
                    circleRadius * 2, circleRadius * 2);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            gfx.DrawEllipse(new XSolidBrush(XColors.LightGray), 
                circleX - circleRadius, circleY - circleRadius, 
                circleRadius * 2, circleRadius * 2);
        }

        // Draw name and title
        gfx.DrawString($"{name} {surname}", titleFont, new XSolidBrush(white), 250, 70);
        gfx.DrawString("Circulium Vİtae", subtitleFont, new XSolidBrush(white), 250, 100);

        // Left column
        var leftX = 50;
        var y = 220;

        // My Contact section
        gfx.DrawString("My Contact", headingFont, new XSolidBrush(burgundy), leftX, y);
        y += 30;
        gfx.DrawString(email, normalFont, XBrushes.Black, leftX, y);
        y += 20;
        gfx.DrawString(phone, normalFont, XBrushes.Black, leftX, y);
        y += 20;
        gfx.DrawString(address, normalFont, XBrushes.Black, leftX, y);

        // Hard Skills section
        y += 40;
        gfx.DrawString("Hard Skills", headingFont, new XSolidBrush(burgundy), leftX, y);
        y += 30;
        foreach (var skill in skills.Split(','))
        {
            gfx.DrawString("• " + skill.Trim(), normalFont, XBrushes.Black, leftX, y);
            y += 20;
        }

        // Right column
        var rightX = 300;
        y = 220;

        // About Me section
        gfx.DrawString("About Me", headingFont, new XSolidBrush(burgundy), rightX, y);
        y += 30;
        gfx.DrawString(aboutText, normalFont, XBrushes.Black, rightX, y, new XStringFormat { LineAlignment = XLineAlignment.Near });

        // Professional Experience
        y += 60;
        gfx.DrawString("Professional Experience", headingFont, new XSolidBrush(burgundy), rightX, y);
        y += 30;
        gfx.DrawString($"School: {school}", normalFont, XBrushes.Black, rightX, y);
         y += 30;
        gfx.DrawString($"Department: {department}", normalFont, XBrushes.Black, rightX, y);
        y += 20;
        gfx.DrawString($"Start Date: {startDate:d}", normalFont, XBrushes.Black, rightX, y);
        y += 20;
        gfx.DrawString($"Finish Date: {finishDate:d}", normalFont, XBrushes.Black, rightX, y);
        using var ms = new MemoryStream();
        document.Save(ms);
        ms.Position = 0;
        return File(ms.ToArray(), "application/pdf", "cv.pdf");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
