#pragma warning disable CA1416

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataPacker.Serialization;
using System.Numerics;
using DataPacker;
using System.Text;
using Tester.Testing;
using static System.String;

namespace Tester;

public class Tester
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Begin tests...\n");

        var tests = new List<(string, Action)>
        {
            ("Simple Classes", TestSimpleClasses),
            ("Simple List", () => TestSimpleList(false)),
            ("Simple List", () => TestSimpleList(true)),
            ("Arrays", TestArrays),
            ("References", TestSelfReference),
            ("IndexedWriter", TestIndexed),
            ("Indexed Advanced 1", TestIndexedAdvanced),
            ("Indexed Advanced 2", TestIndexedAdvanced2),
       //     ("Massive List", TestMassiveList)
        };

        var passed = 0;
        foreach (var (name, test) in tests)
        {
            try
            {
                test.Invoke();
                ++passed;

                Console.WriteLine($"Test {passed}/{tests.Count} passed! -> {name}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"\nTest {name} failed!");
            }
        }
    }

    private static void TestSimpleClasses()
    {
        var simplePacket = new RotationPacket(1337, 102.450019, -2.0);
        var packetBytes = CompactFormatter.Serialize(simplePacket);
        var serverPacket = CompactFormatter.Deserialize<RotationPacket>(packetBytes);
        if (serverPacket.playerId != 1337 || serverPacket.pitch != -2.0)
            throw new Exception();

        var pointers = new Pointers();
        var simple = new Simple("Alice", 22, 1337, "wants Bobs attention!");
        var pi = 3.141592653589793238;
        var info = "Testing 123. Hello !?";

        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.Sequential, Encoding.ASCII);

        writer.Add(CompactFormatter.Serialize(simple));
        writer.Add(CompactFormatter.Serialize(pointers));
        writer.Add(pi);
        writer.Add(info);
        writer.Flush(false);

        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.Sequential, Encoding.ASCII, true);

        if (simple.description != CompactFormatter.Deserialize<Simple>(reader[0].data).description ||
            pointers.ip2 != CompactFormatter.Deserialize<Pointers>(reader[1].data).ip2 ||
            pi != reader[2].ToDouble() ||
            info != reader[3].ToString())
            throw new Exception();


        /*var sw = Stopwatch.StartNew();
        using var basic = new BasicFormatter();
        for (var i = 0; i < 1000000; i++)
        {
            var bt = basic.Serialize(simplePacket); // ~0.0007 ms
        }
        var time = sw.ElapsedMilliseconds;
        Console.WriteLine($"Time {time} ms");
        Environment.Exit(-1);*/
    }

    private static void TestSimpleList(bool named)
    {
        var list = new List<TestEnum>
        {
            TestEnum.NotQuite,
            TestEnum.TryAgain,
            TestEnum.Yes,
            TestEnum.Right,
            TestEnum.Yes
        };

        if (named)
            list.Sort((a, b) => CompareOrdinal(a.ToString(), b.ToString()));

        var before = CompactFormatter.Serialize(list);
        using var ms = new MemoryStream();

        SequenceWriter writer;

        if (named)
        {
            writer = new SequenceWriter(ms, DataStructure.SequentialNamed, Encoding.ASCII);
            writer.Add("The list", before);
        }
        else
        {
            writer = new SequenceWriter(ms, DataStructure.Sequential, Encoding.ASCII);
            writer.Add(before);
        }

        writer.Flush(false);
        writer.Dispose();

        ms.Position = 0;

        List<TestEnum> listRecovered;

        if (named)
        {
            using var reader = new SequenceReader(ms, DataStructure.SequentialNamed, Encoding.ASCII, true);
            listRecovered = CompactFormatter.Deserialize<List<TestEnum>>(reader["The list"].data);
        }
        else
        {
            using var reader = new SequenceReader(ms, DataStructure.Sequential, Encoding.ASCII, true);
            listRecovered = CompactFormatter.Deserialize<List<TestEnum>>(reader[0].data);
        }

        if (listRecovered.Count != 5) throw new Exception();
    }

    private static void TestArrays()
    {
        var arr2 = new Arrays2();
        var bytes2 = CompactFormatter.Serialize(arr2);
        var arr2Rec = CompactFormatter.Deserialize<Arrays2>(bytes2);

        if (arr2Rec.rotations[6].pitch != 51.3 || arr2Rec.str != "Passed?") throw new Exception();

        var arr = new ArraysTest
        {
            simple1 = new Vector2[] { new(1.14f, 2.3f), new(-4f, 90f) },
            simple2 = new[] { "Yes", "No" },
            multi = new[] { new[] { 55m, 34m }, new[] { 12m, 13m }, new[] { 88m, 99m }, new[] { 777m, 0m } },
            name = "Alice",
            age = 21,
            vector = new Vector2(19.8024f, 1.40124445f),
            testing = TestEnum.Yes
        };

        var bytes = CompactFormatter.Serialize(arr);
        var arrRec = CompactFormatter.Deserialize<ArraysTest>(bytes);

        if (arrRec.vector.X != arr.vector.X ||
            arrRec.simple2[1] != arr.simple2[1] ||
            arrRec.multi[3][0] != arr.multi[3][0]) throw new Exception();
    }

    private static void TestSelfReference()
    {
        var challenge = new ReferenceProblem();
        var bytes = CompactFormatter.Serialize(challenge);
        var challengeSolved = CompactFormatter.Deserialize<ReferenceProblem>(bytes);

        if (challengeSolved.someChild.stackOverflow != challengeSolved ||
            challengeSolved.someChild.me != challengeSolved.someChild ||
            challengeSolved.someChild.solved != "yes") throw new Exception();
        ;
    }

    private static void TestIndexed()
    {
        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.Indexed, Encoding.Unicode);

        writer.Add("Hello World!");
        writer.Add(33);
        writer.Add(42);
        writer.Add("Alice?");
        writer.Add(69);
        writer.Add('\n');
        writer.Add("End");

        writer.Flush(false);
        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.Indexed, Encoding.Unicode);

        // Read range
        reader.Read(2, 5, false);

        var num1 = reader[0].ToInt32();
        var name = reader[1].ToString();
        var num2 = reader[2].ToInt32();
        var chr = reader[3].ToChar();

        if (num1 != 42 || name != "Alice?" || num2 != 69 || chr != '\n') throw new Exception();

        reader.Entries.Clear();
        reader.ReadOne(0, false);
        reader.ReadOne(6);

        var first = reader[0].ToString();
        var last = reader[1].ToString();

        if (first != "Hello World!" || last != "End") throw new Exception();
    }

    private static void TestIndexedAdvanced()
    {
        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.IndexedNamed, Encoding.ASCII);

        writer.Add("One", "Hello World!");
        writer.Add("Two", int.MaxValue);
        writer.Add("Three", 42);
        writer.Add("Four", "Alice?");

        writer.Flush(false);
        ms.Position = 0;

        using var reader = new SequenceReader(ms, DataStructure.IndexedNamed, Encoding.ASCII);
        reader.Read(1, 2, false);

        if (reader["Two"].ToInt32() != int.MaxValue || reader["Three"].ToInt32() != 42) throw new Exception();

        ms.Position = 0;

        // Append new data
        using var writerAppend = new SequenceWriter(ms, DataStructure.IndexedNamed, Encoding.ASCII, reader);

        writerAppend.Add("e5", 69);
        writerAppend.Add("e6", '\n');
        writerAppend.Add("e7", "End");

        writerAppend.Flush(false);

        ms.Position = 0;
        using var reader2 = new SequenceReader(ms, DataStructure.IndexedNamed, Encoding.ASCII);
        var av = reader2.Available();
        reader2.Read();

        if (reader2["e6"].ToChar() != '\n' || reader2["e5"].ToInt32() != 69) throw new Exception();

    }

    private static void TestIndexedAdvanced2()
    {
        using var ms = new MemoryStream();
        using var writer = new SequenceWriter(ms, DataStructure.Indexed);

        writer.Add("Hello World!");
        writer.Add(int.MaxValue);
        writer.Add((ushort)12);
        writer.Add(double.MinValue);

        writer.Flush(false);
        ms.Position = 0;

        using var bookReader = new SequenceReader(ms, DataStructure.Indexed);
        using var apWriter = new SequenceWriter(ms, DataStructure.Indexed, appendReader: bookReader);
        apWriter.Add("New Data!");
        apWriter.Add(long.MaxValue);
        apWriter.Add("Passed!");

        apWriter.Flush(false);

        ms.Position = 0;
        using var verifyReader = new SequenceReader(ms, DataStructure.Indexed, autoRead: true);
        if (verifyReader[4].ToString() != "New Data!" || verifyReader[6].ToString() != "Passed!") throw new Exception();
    }

    private static void TestMassiveList()
    {
        var rnd = new Random();
        var strings = new List<StringObject>();
        const int size = 1000000;
        const string set = "abcdefghijklmnopqrstuvwxyz0123456789";
        var len = set.Length;

        for (var i = 0; i < size; i++)
        {
            var sb = new StringBuilder();
            for (var j = 0; j < 3; j++) // do collision testing
                sb.Append(set[rnd.Next(len)]);

            var str = sb.ToString();
            strings.Add(new StringObject(str));
        }

        using var formatter = new BasicFormatter();
        var sw = Stopwatch.StartNew();
        var bytes = formatter.Serialize(strings);
        var ms = sw.ElapsedMilliseconds;

        Console.WriteLine($"Time {ms} ms");
        strings.Clear();

        sw.Restart();
        var u2 = formatter.Deserialize<List<StringObject>>(bytes);
        ms = sw.ElapsedMilliseconds;
        Console.WriteLine($"Time {ms} ms");
        if (u2.Count != size) throw new Exception();
    }

    private static void DebugTesting()
    {
        var num = 1304;

        unsafe
        {
            /*
            var pt = (byte*)&num;
            var b0 = pt[0];
            var b1 = pt[1];
            var b2 = pt[2];
            var b3 = pt[3];

            Console.WriteLine($"{b0} {b1} {b2} {b3}");
            */

            for (var i = 0; i < 10; i++)
            {
                var sw = Stopwatch.StartNew();
                for (var j = 0; j < 1000000; j++)
                {
                    var bt = new byte[5];
                    var pt = (byte*)&j;
                    bt[0] = 1;
                    bt[1] = pt[0];
                    bt[2] = pt[1];
                    bt[3] = pt[2];
                    bt[4] = pt[3];
                }

                var time = sw.ElapsedMilliseconds;
                Console.WriteLine($"Time: {time} ms");

            }

            /*var bt = new byte[5];
                    bt[0] = 1;
                    bt[1] = (byte)((j >> 0) & 0xff);
                    bt[2] = (byte)((j >> 8) & 0xff);
                    bt[3] = (byte)((j >> 16) & 0xff);
                    bt[4] = (byte)((j >> 24) & 0xff);

            Console.WriteLine($"{b4} {b5} {b6} {b7}");*/
        }
    }
}