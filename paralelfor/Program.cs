﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parallelfor
{
    internal class Program
    {
        private static readonly object kilit = new object();
        static void Main(string[] args)
        {
            //pi1();
            //pi2();
            //Ornek1();
            //Ornek2();
            //Ornek4();
            //Ornek5();
            Ornek6();
            Console.ReadLine();
        }
        static void pi1()
        {
            //pi = 4(1-1/3+1/5-1/7+1...)

            //normali
            double pi = 1;
            int t = -1;
            for (int i = 3; i < 2000000; i = i + 2)
            {
                pi = pi + (double)1 / i * t;
                t = t * -1;
            }
            Console.WriteLine(pi*4);

            pi = 1;
            Parallel.For<double>(1, 1000000, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () => 0,
                (i, loop, localState) =>
                {
                    int cmt = i * 2 + 1;
                    //1 2 3 4 5
                    //3 5 7 9 11
                    if (i % 2 == 0)
                        return localState + (double)1 / cmt;
                    else
                        return localState - (double)1 / cmt;
                },
                localState =>
                {
                    pi = pi + localState;
                });
            Console.WriteLine(pi * 4);
        }
        
        static void pi2()
        {
            //pi = 3 + 4/(2*3*4) - 4/(5*6*7) + 4/(8*9*10) - ...

            //normal
            double pi = 3;
            int t = 1;
            for (int i = 2; i < 2000000; i = i + 3)
            {
                pi = pi + (double)4 / (i * (i + 1) * (i + 2)) * t;
                t = t * -1;
            }
            Console.WriteLine(pi);

            Parallel.For<double>(1, 1000000, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () => 0, (i, loop, localState) =>
                {
                    
                    int cmt = (int)i * 2;
                    if (i % 2 == 0)
                        return localState - (double)4 / (cmt * (cmt + 1) * (cmt + 2));
                    else
                        return localState + (double)4 / (cmt * (cmt + 1) * (cmt + 2));
                },
                localState =>
                {
                    lock(kilit)
                        pi = pi + localState;
                });
            Console.WriteLine(pi);
        }

        static void Ornek1() //1 - 100 e kadar tek sayı toplamı
        {
            int toplam = 0;
            for (int i = 1; i < 100; i = i + 2)
            {
                toplam = toplam + i;
            }
            Console.WriteLine(toplam);

            toplam = 0;
            Parallel.For(0, 50, (i) =>
            {
                int tek = i * 2 + 1;
                toplam = toplam + tek;
            });
            Console.WriteLine(toplam);
        }

        static void Ornek2() //-2 +3 -4 +5 ...
        {
            int toplam = 0;
            int t = -1;
            for (int i = 2; i < 100; i++)
            {
                toplam = toplam + i * t;
                t = t * -1;
            }
            Console.WriteLine(toplam);

            toplam = 0;
            Parallel.For(2, 100, (i) =>
            {
                if (i % 2 == 0)
                    toplam = toplam - i;
                else
                    toplam = toplam + i;
            });
            Console.WriteLine(toplam);
        }

        static void Ornek3() //1 - 2 - 3 + 4 + 5 + 6 - 7 - 8 - 9 - 10 + 11 + 12 + 13 + 14... BUNUN PARALEL.FOR OLMAZ
        {//1 -2 3 -4 5 -6
            int toplam = 0;
            int sayac = 1;
            int t = 0;
            for (int i = 1; i < 100; i++)
            {
                if (sayac % 2 == 0)
                    toplam = toplam - i;
                else
                    toplam = toplam + i;
                t++;

                if (t == sayac) { 
                    sayac++;
                    t = 0;
                 }
            }
            Console.WriteLine(toplam);
        }

        static void Ornek4() // 10 faktoriyel hesapla
        {
            int toplam = 1;
            for (int i = 1; i < 11; i++)
            {
                toplam = toplam * i;
            }
            Console.WriteLine(toplam);

            toplam = 1;
            Parallel.For(1, 11, (i) =>
            {
                toplam = toplam * i;
            });
            Console.WriteLine(toplam);
        } 

        static void Ornek5() //1 - x^2/2! + x^4/4! - x^6/6! + .....
        {
            //normal
            double sayi = 1;
            int t = -1;
            int x = 1;

            for (int i = 2; i < 30; i = i + 2)
            {
                sayi = sayi + Math.Pow(x,i) / fac(i) * t;
                t = t * -1;
            }
            Console.WriteLine(sayi);

            sayi = 1;
            //parallelfor:
            Parallel.For<double>(1,16, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                () => 0,
                (i, loop, localState) =>
                {
                    int cmt = i * 2;
                    if (i % 2 == 0)
                        return localState + Math.Pow(x, cmt) / fac(cmt);
                    else
                        return localState - Math.Pow(x, cmt) / fac(cmt);
                },
                localState =>
                {
                    sayi = sayi + localState;
                });
            Console.WriteLine(sayi);
        }

        static int fac(int number)
        {
            int sonuc = 1;
            for (int i = 1; i <= number; i++)
            {
                sonuc = sonuc * i;
            }
            return sonuc;
        }

        static void Ornek6() //mesela 153 -> 1^3 + 5^3 + 3^3
        {
            int a = 153;
            int b = a;
            int basamak = 0;
            for (int i = 0; i < 10; i++)
            {
                b = b / 10;
                basamak++;
                if (b == 0){
                    break;
                }
            }
            int sonuc = 0;
            b = a;
            for (int i = basamak; i > 0; i--)
            {
                int c = b / (int)(Math.Pow(10, i - 1));
                sonuc = sonuc + (int)Math.Pow(c,basamak);

                b = b - (c*(int)Math.Pow(10, i - 1));
            }

            if (a == sonuc)
            {
                Console.WriteLine("Bu bir bilmem ne sayı işte");
            }


            //Paralel 
        }
    }
}
