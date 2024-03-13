using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SourceAFIS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace ImageUploadAPI.Controllers;


public class ImgInfo
{
    public string Finger { get; set; }
    public string EmployeeID { get; set; }
}

[Route("api/[controller]/[action]")]
[ApiController]
public class ImageUploadController : ControllerBase
{
    /*
    [HttpPost]
    public string Test([FromForm]ImgInfo info)
    {
        return System.Text.Json.JsonSerializer.Serialize(info); 
    }
    */
/*
    [HttpPost("{employeeId}/{finger}")]
    public string SaveFinger([FromForm] IFormFile file, [FromRoute] string employeeId, [FromRoute] string finger)
    {
        var stream = new MemoryStream((int)file.Length);
        file.CopyTo(stream);
        byte[] bytes = stream.ToArray();


        var filename = employeeId + "-" + finger;
        return filename;
    }
*/
    [HttpPost()]
    public string SaveFinger([FromForm] IFormFile file, [FromForm] ImgInfo imgInfo)
    {
        var stream = new MemoryStream((int)file.Length);
        file.CopyTo(stream);
        byte[] bytes = stream.ToArray();
        var filename = imgInfo.EmployeeID + "-" + imgInfo.Finger;

        //  böylesi daha iyi demin python'ın files attribute'nu unuttum ilk böyle yapmaya çalışmıştım 
        // neyse sonuç olarak buradan devam edersin
        // alternatif olarak router'dan bilgi almak için 1. örneğe bakabilirsin

        try
        {
            Template.SaveTemplate(bytes, filename);
            return "Kayıt Başarılı";

        }

        catch
        {
            return "Kayıt Başarısız";

        }
    }

    [HttpGet]

    public string FindFinger(string id)
    {
        string[] a = DB.IDTemplFind(id);
        if (a == null)
        {
            return null;
        }
        else
        {
            string b = string.Join(" ", a);
            return b;
        }
    }



    [HttpGet]

    public bool DeleteFinger(string file)
    {
        return DB.FingerDelete(file);
    }


    [HttpPost]

    //public string UploadImage(byte[] bytes)
    public string MatchFinger([FromForm] IFormFile file)
    {
        try
        {
            // getting file original name
            //string FileName = file.FileName;

            // combining GUID to create unique name before saving in wwwroot
            //string uniqueFileName = Guid.NewGuid().ToString() + "_" + FileName;

            // getting full path inside wwwroot/probe
            //var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/probe/", FileName);

            // copying file
            //file.CopyTo(new FileStream(imagePath, FileMode.Create));

            var stream = new MemoryStream((int)file.Length);
            file.CopyTo(stream);
            byte[] bytes = stream.ToArray();


            FingerprintTemplate probe = new FingerprintTemplate(new FingerprintImage(bytes));
            //var ready = Matcher.ReadyTemplates();
            //var found = Matcher.Identify(probe, ready);

            var found = Matcher.Identify(probe, Matcher.ReadyTemplates());



            if (found != null)
            {
                string result = "ID:" + found.EmployeeId + " \n Finger: " + found.Finger;
                return result;
            }
            else
            {
                return "Eslesmedi";
            }


        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}

