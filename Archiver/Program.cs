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
            var bytes = File.ReadAllBytes("text.txt");
            GetBinaryTree(bytes);
            // foreach (var b in bytes)
            // {
            //     Console.Write("[{0}]", b);
            // }
            //
            // Console.WriteLine();
            // Console.WriteLine(ascii.GetString(bytes));
        }

        static void GetBinaryTree(byte[] bytes)
        {
            var dictionary = new Dictionary<byte, int>();
            var root = new Node();
            foreach (var b in bytes)
            {
                if (!dictionary.ContainsKey(b))
                {
                    dictionary.Add(b, 1);
                }
                else
                {
                    dictionary[b] += 1;
                }
            }

            foreach (var key in dictionary.Keys)
            {
                Console.WriteLine("Key: {0} Freq: {1} Val: {2}", key, dictionary[key], Convert.ToChar(key));
            }

            foreach (var key in dictionary.Keys)
            {
                
            }
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

            Console.WriteLine(root.Value);
        }
    }

    class Node
    {
        public Node Left { get; set; }
        public Node Right { get; set; }
        public char Value { get; set; }
        public int Frequence { get; set; }
        public bool IsRoot { get; set; }

        public Node(char value, bool isRoot)
        {
            Value = value;
            IsRoot = isRoot;
        }
        
        public Node(){}

        public void add(Node node)
        {
            if (Left == null)
                Left = node;
            else
            {
                if (Left.Frequence <= node.Frequence)
                    Right = node;
                else
                {
                    Right = Left;
                    Left = node;
                }
            }
            Frequence += node.Frequence;
        }
    }
}