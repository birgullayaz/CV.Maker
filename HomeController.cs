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
        [FromForm] string aboutText, [FromForm] string[] experienceNames,
        [FromForm] DateTime[] startDates, [FromForm] DateTime[] finishDates,
        [FromForm] string[] experienceDescriptions, [FromForm] IFormFile photo)
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
        double circleX = 80;
        double circleY = 60;
        double circleRadius = 45;

        try
        {
            if (photo != null && photo.Length > 0)
            {
                using var stream = new MemoryStream();
                await photo.CopyToAsync(stream);
                stream.Position = 0;
                
                using var image = XImage.FromStream(stream);

                // Calculate dimensions for a perfect circle
                double finalWidth = circleRadius * 2;
                double finalHeight = circleRadius * 2;

                // Center the image
                double xPos = circleX - circleRadius;
                double yPos = circleY - circleRadius;

                // Create circular clipping
                var state = gfx.Save();
                var path = new XGraphicsPath();
                path.AddEllipse(xPos, yPos, finalWidth, finalHeight);
                gfx.IntersectClip(path);

                // Draw the image
                gfx.DrawImage(image, xPos, yPos, finalWidth, finalHeight);
                gfx.Restore(state);

                // Add white border
                gfx.DrawEllipse(new XPen(XColors.White, 3), 
                    xPos, yPos, finalWidth, finalHeight);
            }
            else
            {
                // Placeholder circle if no photo
                gfx.DrawEllipse(new XSolidBrush(XColors.LightGray), 
                    35, 15, 90, 90);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            // Placeholder circle if error
            gfx.DrawEllipse(new XSolidBrush(XColors.LightGray), 
                35, 15, 90, 90);
        }

        // Draw name and title
        gfx.DrawString($"{name} {surname}", titleFont, new XSolidBrush(white), 180, 70);
        gfx.DrawString("Curriculum Vitae", subtitleFont, new XSolidBrush(white), 180, 100);

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

        for (int i = 0; i < experienceNames.Length; i++)
        {
            gfx.DrawString($"Experience: {experienceNames[i]}", normalFont, XBrushes.Black, rightX, y);
            y += 20;
            gfx.DrawString($"Period: {startDates[i]:d} - {finishDates[i]:d}", normalFont, XBrushes.Black, rightX, y);
            y += 20;
            gfx.DrawString(experienceDescriptions[i], normalFont, XBrushes.Black, rightX, y);
            y += 30;
        }

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
