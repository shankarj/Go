using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace simluationProject
{
    //CUSTOM DATA
    public class cutItem
    {
        public int X = 0;
        public int Y = 0;
    }

    public class nodeItem
    {
        public int nodeNumber;
        public int[,] boardConfig;
        public int winCount = 0;
        public int visitCount = 0;
        public int parentNodeNumber;
    }


    class Program
    {
        const int BOARDSIZE = 5;
        static int[,] currentBoardConfig = new int[BOARDSIZE, BOARDSIZE];
        static int[,] mask = new int[BOARDSIZE, BOARDSIZE];

        static Stopwatch tempWatch = new Stopwatch();

        static Random moveGenX = new Random();
        static Random moveGenY = new Random();

        static List<cutItem> groupFound = new List<cutItem>();

        static void Main(string[] args)
        {
            tempWatch.Start();

            StreamReader tempReader = new StreamReader(Environment.CurrentDirectory + "\\input3.txt");

            initializeBoard();

            for (int i = 0; i < BOARDSIZE; i++)
            {
                string temp = tempReader.ReadLine();

                for (int j = 0; j < BOARDSIZE; j++)
                {
                    currentBoardConfig[i, j] = int.Parse(Convert.ToString(temp[j]));
                }
            }

            tempReader.Close();

            int[,] tempOutput = captureCoins(currentBoardConfig, 2);

            groupFind(2, 3, 1);

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    Console.Write(tempOutput[i, j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine(tempWatch.Elapsed);
            Console.ReadLine();

        }

        static void initializeBoard()
        {
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    currentBoardConfig[i, j] = 0;
                    mask[i, j] = 0;
                }
            }
        }

        static void initializeMask()
        {
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    mask[i, j] = 0;
                }
            }
        }

#region "TO CAPTURE"
        static void groupFind(int x, int y, int playedBy)
        {
            if ((x < 0) || (x >= BOARDSIZE) || (y < 0) || (y >= BOARDSIZE))
                return;

            if ((currentBoardConfig[x, y] != playedBy) && (mask[x, y] == 0))
            {
                mask[x, y] = 1;

                cutItem tempItem = new cutItem();
                tempItem.X = x;
                tempItem.Y = y;

                groupFound.Add(tempItem);

                // look at the neighbours
                groupFind(x + 1, y, playedBy);
                groupFind(x - 1, y, playedBy);
                groupFind(x, y + 1, playedBy);
                groupFind(x, y - 1, playedBy);

            }
        }

        static int[,] captureFromGroup(int[,] currentBoard, List<cutItem> groupItems, int playedBy)
        {
            bool isSafe = false;
            bool oneHasLiberty = false;
            int[,] tempBoard = currentBoard;

            for (int i = 0; i < groupItems.Count; i++)
            {
                if (HasLiberty(groupItems[i].X, groupItems[i].Y, tempBoard))
                {
                    oneHasLiberty = true;
                    break;
                }
            }

            if (!(oneHasLiberty))
            {
                for (int j = 0; j < groupItems.Count; j++)
                {
                    tempBoard[groupItems[j].X, groupItems[j].Y] = 0;
                }
            }

            return tempBoard;
        }

        static bool HasLiberty(int X, int Y, int[,] currentBoard)
        {
            int noSafeCount = 0;

            for (int loopCount = 0; loopCount < 4; loopCount++)
            {
                int TX = 0;
                int TY = 0;

                switch (loopCount)
                {
                    case 0:
                        TX = X + 1;
                        TY = Y;
                        break;
                    case 1:
                        TX = X - 1;
                        TY = Y;
                        break;
                    case 2:
                        TX = X;
                        TY = Y + 1;
                        break;
                    case 3:
                        TX = X;
                        TY = Y - 1;
                        break;
                }

                if ((TX >= 0) && (TX < BOARDSIZE) && (TY >= 0) && (TY < BOARDSIZE))
                {
                    if (currentBoard[TX, TY] != 0)
                    {
                        noSafeCount += 1;
                    }

                }
                else
                {
                    noSafeCount += 1;
                }
            }

            if (noSafeCount == 4)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        static int[,] captureCoins(int[,] currentBoard, int playedBy)
        {
            int[,] tempBoard = currentBoard;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if ((tempBoard[i, j] != 0) && (tempBoard[i, j] != playedBy))
                    {
                        initializeMask();
                        groupFound.Clear();
                        groupFind(i, j, playedBy);
                        tempBoard = captureFromGroup(tempBoard, groupFound, playedBy);
                    }
                }
            }

            return tempBoard;
        }
       
#endregion


        static int[,] createRandomPlay(int[,] currentBoard, int toPlay)
        {

            int[,] tempBoard = currentBoard;
            int X = 0;
            int Y = 0;

            do
            {
                X = moveGenX.Next(10);
                Y = moveGenY.Next(10);

            } while (tempBoard[X, Y] != 0);

            tempBoard[X, Y] = toPlay;

            return tempBoard;
        }
    }
}
