using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Archiver
{
    public class Archiver
    {
        public static void Encode(string source, string direction)
        {
            var eFile = new EncodedFile();
            GetCodeDict(GetBinaryTree(File.ReadAllBytes(source)), eFile.codeDictionary);
            eFile.encodedBytes = GetEncodedBytes(File.ReadAllText(source), eFile);
            SaveInFile(eFile, direction);
        }
        
        public static void ShowCompressCoef(string source, string direction)
        {
            var sourceWeight = (double) new FileInfo(source).Length;
            var directionWeight = (double) new FileInfo(direction).Length;
            Console.WriteLine("Коэффициент сжатия составляет: {0}", sourceWeight / directionWeight);
        }

        static void SaveInFile(EncodedFile eFile, string path)
        {
            var bf = new BinaryFormatter();
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                bf.Serialize(fs, eFile);
                Console.WriteLine("Файл заархивирован");
            }
        }

        static EncodedFile ExtractFromFile(string path)
        {
            var bf = new BinaryFormatter();
            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                var eFile = (EncodedFile) bf.Deserialize(fs);
                return eFile;
            }
        }

        public static void Decode(string direction, string newDirection)
        {
            var eFile = ExtractFromFile(direction);
            var dictionaryForDecode = new Dictionary<string, char>();
            var binaryStr = StringToBinary(eFile.encodedBytes.ToArray());
            binaryStr = binaryStr.Substring(0, binaryStr.Length - eFile.countOfZeros);
            foreach (var c in eFile.codeDictionary)
            {
                dictionaryForDecode.Add(c.Value, c.Key);
            }

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
                else
                {
                    throw new ArgumentOutOfRangeException(partOfMsg, "Строка не является началом ни одного ключа");
                }
            }
            File.WriteAllText(newDirection, result.ToString());
            Console.WriteLine("Файл разархивирован в " + newDirection);
        }

        public static string StringToBinary(byte[] data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char b in data)
            {
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return sb.ToString();
        }

        public static List<byte> GetEncodedBytes(string text, EncodedFile eFile)
        {
            var newBytes = new List<byte>();
            var encodedString = new StringBuilder();
            foreach (var elem in text)
            {
                encodedString.Append(eFile.codeDictionary[elem]);
            }

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

        public static Node GetBinaryTree(byte[] bytes)
        {
            var dictionary = GetFreqDict(bytes);
            var queue = new PriorityQueue();
            foreach (var key in dictionary.Keys)
            {
                queue.Add(new Node(key, dictionary[key]));
            }

            while (queue.GetCount() > 1)
            {
                var firstLeaf = queue.Remove();
                var secondLeaf = queue.Remove();
                var node = new Node();
                node.Add(firstLeaf);
                node.Add(secondLeaf);
                queue.Add(node);
            }

            var result = queue.Remove();
            EncodeTree(result, "", "");
            return result;
        }

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

        static void ShowBinaryTree(Node root)
        {
            if (root.Left != null)
            {
                ShowBinaryTree(root.Left);
            }

            if (root.Right != null)
            {
                ShowBinaryTree(root.Right);
            }

            if (root.Code != null)
                Console.WriteLine("Byte:{0}, Frequency: {1}, Char: {2}, Code: {3}", root.Value, root.Frequency,
                    Convert.ToChar(root.Value), root.Code);
        }
    }
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