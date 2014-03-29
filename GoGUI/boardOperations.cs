using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GoGUI
{
    public class cutItem
    {
        public int X = 0;
        public int Y = 0;
    }

    public class boardOperations
    {
        int BOARDSIZE = 0;
        int[,] mask;

        Random moveGenX = new Random();
        Random moveGenY = new Random();

        bool gotACut = false;

        List<cutItem> groupFound = new List<cutItem>();
        List<cutItem> overallCuts = new List<cutItem>();

        public boardOperations(int boardSize)
        {
            BOARDSIZE = boardSize;
            mask = new int[BOARDSIZE, BOARDSIZE];
        }

        public int[,] initializeBoard(int[,] currentBoard)
        {
            int[,] tempBoard = currentBoard;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    tempBoard[i, j] = 0;
                }
            }

            return tempBoard;
        }

        #region "TO CAPTURE"

        private void initializeMask()
        {
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    mask[i, j] = 0;
                }
            }
        }

        private void groupFind(int x, int y, int playedBy, int[,] currentBoard)
        {
            if ((x < 0) || (x >= BOARDSIZE) || (y < 0) || (y >= BOARDSIZE))
                return;

            if ((currentBoard[x, y] != playedBy) && (mask[x, y] == 0) && (currentBoard[x, y] != 0))
            {
                mask[x, y] = 1;

                cutItem tempItem = new cutItem();
                tempItem.X = x;
                tempItem.Y = y;

                groupFound.Add(tempItem);

                // look at the neighbours
                groupFind(x + 1, y, playedBy, currentBoard);
                groupFind(x - 1, y, playedBy, currentBoard);
                groupFind(x, y + 1, playedBy, currentBoard);
                groupFind(x, y - 1, playedBy, currentBoard);

            }
        }

        private int[,] captureFromGroup(int[,] currentBoard, List<cutItem> groupItems, int playedBy)
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
                    overallCuts.Add(groupItems[j]);
                }

                gotACut = true;
            }

            if (!(gotACut))
            {
                groupFound.Clear();
            }

            return tempBoard;
        }

        private bool HasLiberty(int X, int Y, int[,] currentBoard)
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

        public int[,] captureCoins(int[,] currentBoard, int playedBy)
        {
            int[,] tempBoard = currentBoard;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if ((tempBoard[i, j] != 0) && (tempBoard[i, j] != playedBy))
                    {
                        initializeMask();
                        groupFind(i, j, playedBy, currentBoard);
                        tempBoard = captureFromGroup(tempBoard, groupFound, playedBy);
                    }
                }
            }

            return tempBoard;
        }

        public List<cutItem> returnCuts()
        {
            List<cutItem> tempCuts = new List<cutItem>();

            for (int i = 0; i < overallCuts.Count; i++)
			{
			  tempCuts.Add(overallCuts[i]);
			}

            if (gotACut)
            {
                overallCuts.Clear();
                gotACut = false;
                return tempCuts;
            }

            return null;
        }

        #endregion
    }
}
