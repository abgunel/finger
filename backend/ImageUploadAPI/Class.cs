using Dahomey.Cbor;
using SourceAFIS;
using System;
using System.Collections.Generic;
using System.IO;
using Dahomey.Cbor.ObjectModel;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.AspNetCore.Diagnostics;

namespace ImageUploadAPI
{
    public static class Template
    {

        //path = path + imagenin ismi + .formatı
        //Imgenin template ini RAM e alma metodu
        public static byte[] ImageToTemplate(string path)
        {

            var template = new FingerprintTemplate(new FingerprintImage(File.ReadAllBytes(path)));
            byte[] serialized = template.ToByteArray();
            return serialized;
        }

        //path = path + template'in ismi+ .formatı 
        public static FingerprintTemplate LoadTemplate(string path)
        {

            var serialized = File.ReadAllBytes(path);
            var template = new FingerprintTemplate(serialized);
            return template;
        }


        //Dosyanın içinde bulduğu test edilecek probe imgenin şablonunu çıkarır ve imgeyi siler.
        // SILDIRMEYI UNCOMMENT LEMEYI UNUTMA!!!
        public static FingerprintTemplate ProbeTemplate()
        {
            string img = DB.ProbeFind();
            var templ = Template.ImageToTemplate(img);
            //File.Delete(img);
            FingerprintTemplate probe = new FingerprintTemplate(templ);
            return probe;

        }

        // Imgeyi şablonlaştırıp templates klasörüne kaydeder ve imgeyi siler.
        public static void SaveTemplate(byte[] img, string name)
        {
            string temp_path = "wwwroot/templates/";
            string name_format = name + ".cbor";
            string save_path = temp_path + name_format;
            FingerprintTemplate templ = new FingerprintTemplate(new FingerprintImage(img));
            byte[] serialized = templ.ToByteArray();
            File.WriteAllBytes(save_path, serialized);
        }

        public static void SaveTemplates()
        {
            string[] img = DB.ImagesFind();

            foreach (var item in img)
            {

                string img_name = Path.GetFileNameWithoutExtension(item);
                string temp_path = "wwwroot/templates/";
                string name_format = img_name + ".cbor";
                string save_path = temp_path + name_format;
                var templ = Template.ImageToTemplate(item);
                File.WriteAllBytes(save_path, templ);
                File.Delete(item);
            }

        }
    }


    public static class DB
    {
        /*
        //Kayitli templ lerin listesini dondur

        public static string[] TemplFind()
        {
            string path = "wwwroot/templates/";
            string[] FPP = Directory.GetFiles(path, "*.cbor");
            string[] FPT= new string[FPP.Length];
            for (int i = 0; i<FPP.Length; i++)
            {
                FPT[i] = Path.GetFileNameWithoutExtension(FPP[i]);
            }

            return FPT;
        }
        */

        //Aranan ID nini templ dondur

        public static string[] IDTemplFind(string id)
        {
            string path = "wwwroot/templates/";
            try
            {
                string name = id + "-*";
                string[] FPP = Directory.GetFiles(path, name);
                string[] TID = new string[FPP.Length];
                string[] TN = new string[FPP.Length];
                for (int i = 0; i<FPP.Length; i++)
                {
                    TN[i]= Path.GetFileNameWithoutExtension(FPP[i]);
                    TID[i] = TN[i].Split('-')[1];
                }
                
                return TID;
            }
            catch
            {
                return null;
            }

        }

        //ismi alinan her bir templ i parse le false ise ID dondur true ise finger
        public static string[] TemplParse(string [] FPT, bool X)
        {

            for (int i=0; i<FPT.Length; i++)
            {
                string[] parse_item = FPT[i].Split('-');
                if (X == false)
                {
                    FPT[i] = parse_item[0];
                }
                if (X == true)
                {
                    FPT[i] = parse_item[1];
                }
            }
            
            return FPT;
        }

        // ID si ayni olanlardan 1 tane tut ve array i dondur
        /*
        public static string[] TemplID()
        {
            string[] FPT = TemplFind();
            string [] Parse =TemplParse(FPT,false);
            string[] ID = Parse.Distinct().ToArray();
            return ID;
        }
        */

        //Istenen ID nin kayitli parmaklarini dondur 

        public static string[] FingerFind(string ID)
        {
            string[] finger = IDTemplFind(ID);
            if (finger == null) return null;
            else
            {
                string[] Parse = TemplParse(finger, true);
                return Parse;
            }

        }

        //istenen ID'nin istenilen parmağını silme ID+ "-" + Finger + ".cbor" seklinde input verilmeli

        public static bool FingerDelete(string name)
        {
            string path = "wwwroot/templates/";
            
            
            if (File.Exists(Path.Combine(path, name)))
            {
                File.Delete(Path.Combine(path, name));
                return true;
            }
            else 
            {
                return false;
            }
            
        }


        public static string[] ImagesFind()
        {
            string Path = "wwwroot/";

            string[] FPP = Directory.GetFiles(Path, "*.bmp");
            return FPP;
        }

        //Kontrol edilecek Parmak izi konumu
        public static string ProbeFind()
        {
            string Path = "wwwroot/probe/";
            string[] SPP = Directory.GetFiles(Path, "*.bmp");
            string PP = SPP[0];
            return PP;
        }


        public static string[] TemplateSearch()
        {


            string Path = "wwwroot/templates/";
            string[] TP = Directory.GetFiles(Path, "*.cbor");
            return TP;
        }


        //HER BİR İNDEKS İÇİN SIRAYLA BURAYA GÖNDER. ??? HATALI VEYA EKSIK???
        public static FingerprintTemplate FPtoTemplate(string FP)
        {
            var templ = new FingerprintTemplate(
        new FingerprintImage(File.ReadAllBytes(FP)));

            return templ;
        }

    }


    public static class Matcher
    {
        public record Subject
        {
            public string EmployeeId;
            public string Finger;
            public FingerprintTemplate Template;
        }

        //Templ bul ve sırayla liste ata
        public static List<Subject> ReadyTemplates()
        {

            var candidates = new List<Subject>();
            string[] templs = DB.TemplateSearch();

            foreach (var item in templs)
            {

                //TIREYI SON HALDE DEGISTIR ALTTAN TIRE DEGIL NORMAL OLACAK!!!

                string FileName = new FileInfo(item).Name;
                string[] parse_item = FileName.Split('-');
                //string [] parse_item = item.Split('_');
                string EmployeeID = parse_item[0];
                string FingerandFormat = parse_item[1];
                string Finger = FingerandFormat.Split(".")[0];
                candidates.Add(new Subject { EmployeeId = EmployeeID, Finger = Finger, Template = Template.LoadTemplate(item) });
            }



            return candidates;
        }



        public static Subject Identify(FingerprintTemplate probe, IEnumerable<Subject> candidates)
        {
            var matcher = new FingerprintMatcher(probe);
            Subject match = null;
            double max = Double.NegativeInfinity;
            foreach (var candidate in candidates)
            {
                double similarity = matcher.Match(candidate.Template);
                if (similarity > max)
                {
                    max = similarity;
                    match = candidate;
                }
            }
            double threshold = 40;
            return max >= threshold ? match : null;
        }

    }
}
