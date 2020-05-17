using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Archiver
{
    class Program
    {
        public static Dictionary<char, string> CodeDict = new Dictionary<char, string>();

        static void Main(string[] args)
        {
            var bytes = File.ReadAllBytes("testfile.txt");
            GetCodeDict(GetBinaryTree(bytes));
            var encodedBytes = GetEncodedString(File.ReadAllText("testfile.txt"));
            // encodedBytes.Insert(0, Convert.ToByte(encodedBytes.Count));
            // foreach (var d in CodeDict)
            // {
            //     var line = d.Key + d.Value + " ";
            //     foreach (var c in Encoding.ASCII.GetBytes(line))
            //     {
            //         encodedBytes.Add(c);
            //     }
            // }
            File.WriteAllBytes("encodedfile.tx", encodedBytes.ToArray());
            var encodeFileText = File.ReadAllBytes("encodedfile.tx");
            var encodeBinaryString = StringToBinary(encodeFileText);
            Console.WriteLine("Bites after encoding:");
            Console.WriteLine(encodeBinaryString);
            Console.WriteLine("Здрасьте, я ваш текст: {0}", ExtractFromString(encodeBinaryString));
        }

        public static string ExtractFromString(string encodedMsg)
        {
            var dictForExtract = new Dictionary<string, char>();
            foreach (var c in CodeDict)
            {
                dictForExtract.Add(c.Value, c.Key);
            }

            var result = new StringBuilder();
            var partOfMsg = "";
            var length = 1;
            while (length < encodedMsg.Length)
            {
                partOfMsg = encodedMsg.Substring(0, length);
                var entries = 0;
                foreach (var d in dictForExtract)
                {
                    if (d.Key.StartsWith(partOfMsg))
                    {
                        entries++;
                    }
                }

                if (entries == 1)
                {
                    result.Append(dictForExtract[encodedMsg.Substring(0, length)]);
                    encodedMsg = encodedMsg.Remove(0, length);
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

            return result.ToString();
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

        public static List<byte> GetEncodedString(string text)
        {
            var newBytes = new List<byte>();
            var encodedString = new StringBuilder();
            foreach (var elem in text)
            {
                // newBytes.Add(Convert.ToByte(CodeDict[elem].PadRight(8,'0'),2));
                encodedString.Append(CodeDict[elem]);
                Console.WriteLine(CodeDict[elem]);
            }

            var zeroCounter = (encodedString.Length % 8 != 0) ? 8 - encodedString.Length % 8 : 0;
            while (zeroCounter != 0)
            {
                encodedString.Append("0");
                zeroCounter--;
            }

            for (int i = 0; i < encodedString.Length; i += 8)
            {
                newBytes.Add(Convert.ToByte(encodedString.ToString().Substring(i, 8), 2));
                Console.WriteLine("Bits:{0}, Byte:{1}", encodedString.ToString().Substring(i, 8),
                    Convert.ToByte(encodedString.ToString().Substring(i, 8), 2));
            }

            Console.WriteLine(encodedString.ToString());
            return newBytes;
        }

        public static void GetCodeDict(Node tree)
        {
            if (tree.Left != null)
            {
                GetCodeDict(tree.Left);
            }

            if (tree.Right != null)
            {
                GetCodeDict(tree.Right);
            }

            if (tree.Code != null)
            {
                CodeDict.Add(Convert.ToChar(tree.Value), tree.Code);
                Console.WriteLine("Byte:{0}, Frequency: {1}, Char: {2}, Code: {3}", tree.Value, tree.Frequency,
                    Convert.ToChar(tree.Value), tree.Code);
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

            // Console.WriteLine("ShowQueue");
            // queue.ShowQueue();

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

            Console.WriteLine("GetFreqDict:");
            foreach (var key in dictionary.Keys)
            {
                Console.WriteLine("Freq: {0}, Value: {1}, Char: {2}", dictionary[key], key, Convert.ToChar(key));
            }

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

    class Node
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

    class BinaryTree
    {
        private Node root;

        public BinaryTree()
        {
            root = new Node();
        }

        public BinaryTree(Node root)
        {
            this.root = root;
        }

        public int GetFrequency()
        {
            return root.Frequency;
        }

        public Node GetRoot()
        {
            return root;
        }
    }

    class PriorityQueue
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

        public void ShowQueue()
        {
            foreach (var elem in data)
            {
                Console.WriteLine("Frequency: {0}, Value: {1}, Char: {2}", elem.Frequency, elem.Value,
                    Convert.ToChar(elem.Value));
            }
        }
    }
}