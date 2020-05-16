using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
        {
            var ascii = Encoding.ASCII;
            var bytes = File.ReadAllBytes("testfile.txt");
            var tree = GetBinaryTree(bytes);
            EncodeTree(tree, "", "");
            Console.WriteLine("ShowBinaryTree:");
            ShowBinaryTree(tree);
            var encodedString = GetEncodedString(File.ReadAllText("testfile.txt"));
        }

        public static string GetEncodedString(string text)
        {
            return text;
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

            Console.WriteLine("ShowQueue");
            queue.ShowQueue();

            while (queue.GetCount() > 1)
            {
                var firstLeaf = queue.Remove();
                var secondLeaf = queue.Remove();
                var node = new Node();
                node.Add(firstLeaf);
                node.Add(secondLeaf);
                queue.Add(node);
            }

            return queue.Remove();
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