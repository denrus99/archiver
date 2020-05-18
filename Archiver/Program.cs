using System;
using Archiver;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var source = "text.txt";
            var direction = "encodedfile.tx";
            //Кодирование и сохранение файла
            Archiver.Encode(source, direction);
            //Расчет коэффициента сжатия
            Archiver.ShowCompressCoef(source, direction);
            //Распаковка файла
            Console.WriteLine("Здрасьте, я ваш текст: {0}", Archiver.Decode(direction));
        }
    }
}