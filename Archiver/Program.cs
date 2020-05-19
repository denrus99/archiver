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
            Console.WriteLine("Введите путь к файлу для архивации");
            var source = Console.ReadLine();
            var direction = source + ".arc";
            //Кодирование и сохранение файла
            Archiver.Encode(source, direction);
            Console.WriteLine("Имя заархивированного файла:" + direction);
            //Расчет коэффициента сжатия
            Archiver.ShowCompressCoef(source, direction);
            //Распаковка файла
            Archiver.Decode(direction, "new" + source);
        }
    }
}