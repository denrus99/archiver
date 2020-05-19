using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Archiver
{
    public class Archiver
    {
        //Основной метод для сжатия/кодирования файла
        public static void Encode(string source, string direction)
        {
            var eFile = new EncodedFile();
            //Получение словаря со значениями символов и кодов для них
            GetCodeDict(GetBinaryTree(File.ReadAllBytes(source)), eFile.codeDictionary);
            //Получение новых байт для записи их в файл
            eFile.encodedBytes = GetEncodedBytes(File.ReadAllText(source), eFile);
            SaveInFile(eFile, direction);
        }
        
        //Метод, который выводит коэффициент сжатия
        public static void ShowCompressCoef(string source, string direction)
        {
            var sourceWeight = (double) new FileInfo(source).Length;
            var directionWeight = (double) new FileInfo(direction).Length;
            Console.WriteLine("Коэффициент сжатия составляет: {0}", sourceWeight / directionWeight);
        }
        
        //Метод записывающий итоговый массив закодированных байт в файл
        static void SaveInFile(EncodedFile eFile, string path)
        {
            var bf = new BinaryFormatter();
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                bf.Serialize(fs, eFile);
                Console.WriteLine("Файл заархивирован");
            }
        }

        //Получает объект, в котором хранится массив закодированных байт и словарь для декодирования
        static EncodedFile ExtractFromFile(string path)
        {
            var bf = new BinaryFormatter();
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                var eFile = (EncodedFile) bf.Deserialize(fs);
                return eFile;
            }
        }

        //Основной метод для декодирования информации из файла
        public static void Decode(string direction, string newDirection)
        {
            var eFile = ExtractFromFile(direction);
            var dictionaryForDecode = new Dictionary<string, char>();
            var binaryStr = StringToBinary(eFile.encodedBytes.ToArray());
            //Вычитаем лишние нули из полученной строки
            binaryStr = binaryStr.Substring(0, binaryStr.Length - eFile.countOfZeros);
            foreach (var c in eFile.codeDictionary)
            {
                dictionaryForDecode.Add(c.Value, c.Key);
            }

            //Декодирование исходного сообщения по словарю
            var result = new StringBuilder();
            var partOfMsg = "";
            var length = 1;
            while (length < binaryStr.Length)
            {
                partOfMsg = binaryStr.Substring(0, length);
                var entries = 0;
                foreach (var d in dictionaryForDecode)
                {
                    if (d.Key.StartsWith(partOfMsg))
                    {
                        entries++;
                    }
                }

                if (entries == 1)
                {
                    result.Append(dictionaryForDecode[binaryStr.Substring(0, length)]);
                    binaryStr = binaryStr.Remove(0, length);
                    length = 1;
                }
                else if (entries > 1)
                {
                    length++;
                }
            }
            File.WriteAllText(newDirection, result.ToString());
            Console.WriteLine("Файл разархивирован в " + newDirection);
        }

        
        //Вспомогательный метод для получения строки, состоящей из "0" и "1".
        public static string StringToBinary(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char b in data)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return sb.ToString();
        }

        
        //Метод, кодирующий текст по составленному словарю
        public static List<byte> GetEncodedBytes(string text, EncodedFile eFile)
        {
            var newBytes = new List<byte>();
            var encodedString = new StringBuilder();
            foreach (var elem in text)
            {
                encodedString.Append(eFile.codeDictionary[elem]);
            }

            //Добавление нулей в конце для того, чтобы длина строки была кратна 8
            var zeroCounter = (encodedString.Length % 8 != 0) ? 8 - encodedString.Length % 8 : 0;
            eFile.countOfZeros = zeroCounter;
            while (zeroCounter != 0)
            {
                encodedString.Append("0");
                zeroCounter--;
            }

            for (int i = 0; i < encodedString.Length; i += 8)
            {
                newBytes.Add(Convert.ToByte(encodedString.ToString().Substring(i, 8), 2));
            }
            return newBytes;
        }

        //Получение словаря <символ, код> из дерева Хаффмана
        public static void GetCodeDict(Node tree, Dictionary<char, string> dictionary)
        {
            if (tree.Left != null)
            {
                GetCodeDict(tree.Left, dictionary);
            }

            if (tree.Right != null)
            {
                GetCodeDict(tree.Right, dictionary);
            }

            if (tree.Code != null)
            {
                dictionary.Add(Convert.ToChar(tree.Value), tree.Code);
                Console.WriteLine("Частота: {0}, Код: {1}, Символ: {2}", tree.Frequency,
                    tree.Code, Convert.ToChar(tree.Value));
            }
        }

        //Добавление кодов в дерево Хаффмана
        public static void EncodeTree(Node tree, string codeBefore, string currentCode)
        {
            if (tree.Right == null && tree.Left == null)
                tree.Code = codeBefore + currentCode;
            if (tree.Left != null)
            {
                EncodeTree(tree.Left, codeBefore + currentCode, "1");
            }

            if (tree.Right != null)
            {
                EncodeTree(tree.Right, codeBefore + currentCode, "0");
            }
        }

        //Составление дерева Хаффмана
        public static Node GetBinaryTree(byte[] bytes)
        {
            //Получаем словарь <символ, частота>
            var dictionary = GetFreqDict(bytes);
            //Создаем приоритетную очередь, в которое самым приоритетным считается символ с самой маленькой частотой
            var queue = new PriorityQueue();
            foreach (var key in dictionary.Keys)
            {
                queue.Add(new Node(key, dictionary[key]));
            }

            while (queue.GetCount() > 1)
            {
                //Берём два первых узла из очереди и соединяем их в новый с суммарной частотой его потомков
                var firstLeaf = queue.Remove();
                var secondLeaf = queue.Remove();
                var node = new Node();
                node.Add(firstLeaf);
                node.Add(secondLeaf);
                //Кладём, получившийся узел в очередь 
                queue.Add(node);
            }

            var result = queue.Remove();
            //Получаем код для каждого символа в дереве
            EncodeTree(result, "", "");
            return result;
        }

        //Метод, возвращающий словарь <символ, частота>
        public static Dictionary<byte, int> GetFreqDict(byte[] bytes)
        {
            var dictionary = new Dictionary<byte, int>();
            foreach (var b in bytes)
            {
                if (!dictionary.ContainsKey(b))
                {
                    dictionary.Add(b, 1);
                }
                else
                {
                    dictionary[b]++;
                }
            }
            Console.WriteLine("Таблица символов и кодов для них:");
            return dictionary;
        }
    }
  
    //Класс, реализующий бинарное дерево
    public class Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public byte Value { get; set; }
        public int Frequency { get; set; }

        public string Code { get; set; }

        public Node(byte value, int frequency)
        {
            Value = value;
            Frequency = frequency;
        }

        public Node()
        {
        }

        public void Add(Node node)
        {
            if (Left == null)
                Left = node;
            else
            {
                if (Left.Frequency <= node.Frequency)
                    Right = node;
                else
                {
                    Right = Left;
                    Left = node;
                }
            }

            Frequency += node.Frequency;
        }
    }

    //Класс, реализующий приоритетную очередь
    public class PriorityQueue
    {
        private List<Node> data;
        private int countElems;

        public PriorityQueue()
        {
            data = new List<Node>();
            countElems = 0;
        }

        public void Add(Node node)
        {
            if (countElems == 0)
                data.Add(node);
            else
            {
                for (int i = 0; i < countElems; i++)
                {
                    if (data[i].Frequency > node.Frequency)
                    {
                        data.Insert(i, node);
                        break;
                    }

                    if (i == countElems - 1)
                        data.Add(node);
                }
            }

            countElems++;
        }

        public Node Remove()
        {
            Node tmp = data[0];
            data.RemoveAt(0);
            countElems--;
            return tmp;
        }

        public int GetCount()
        {
            return countElems;
        }
    }
    
    //Класс, для последующего сохранения в файл
    [Serializable]
    public class EncodedFile
    {
        public List<byte> encodedBytes;
        public int countOfZeros;
        public Dictionary<char, string> codeDictionary;

        public EncodedFile()
        {
            encodedBytes = new List<byte>();
            countOfZeros = 0;
            codeDictionary = new Dictionary<char, string>();
        }
    }
}