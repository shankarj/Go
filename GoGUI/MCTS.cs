using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace GoGUI
{
    class node
    {
        public int nodeNumber;
        public int[,] boardConfig;
        public List<node> childNodes = new List<node>();
        public node parentNode;
        public int value = 0;
        public int visitCount = 0;
        public int availableActions = 0;
        public int nextPlayer = 0;
        public int moveX = 0;
        public int moveY = 0;
        public bool isRoot = false;

    }

    public class MCTS
    {
        int BOARDSIZE = 0;
        public int currentNodeNumber = 1;
        int currentTurn = 2;

        int horizonCount = 0;

        node ROOTNODE;

        Random moveGenX = new Random();
        Random moveGenY = new Random();

        Random pickMove = new Random();

        boardOperations boardOps;

        public MCTS(int boardSize, int[,] currentBoard)
        {
            //ROOT NODE INITIALIZED
            BOARDSIZE = boardSize;
            ROOTNODE = new node();
            ROOTNODE.boardConfig = new int[BOARDSIZE, BOARDSIZE];
            ROOTNODE.nodeNumber = currentNodeNumber;
            ROOTNODE.nextPlayer = currentTurn;

            ROOTNODE.visitCount += 1;
            ROOTNODE.isRoot = true;

            currentNodeNumber += 1;

            copyArray(currentBoard, ROOTNODE.boardConfig);

            boardOps = new boardOperations(BOARDSIZE);
        }

        public List<int> getNextMove(Stopwatch computerTimer, double maxTime)
        {
            List<int> nextMoves = new List<int>();

            node tempNode;

            horizonCount = 0;

            double maxValue = 0.0;
            int bestNode = 0;
            bool HasLiberal;

            computerTimer.Restart();

            while (Convert.ToDouble(computerTimer.Elapsed.Seconds) <= maxTime)
            {
                tempNode = ROOTNODE;
                tempNode.availableActions = getAvailableActions(tempNode);

                HasLiberal = false;

                for (int i = 0; i < BOARDSIZE; i++)
                {
                    for (int j = 0; j < BOARDSIZE; j++)
                    {
                        if (tempNode.boardConfig[i, j] == 0)
                        {
                            if (isLiberalMove(i, j, tempNode.boardConfig, currentTurn))
                            {
                                HasLiberal = true;
                                break;
                            }
                        }
                    }

                    if (HasLiberal)
                    {
                        break;
                    }
                }

                if (!(HasLiberal))
                {
                    nextMoves.Add(5000);
                    return nextMoves;
                }

                while (tempNode.childNodes.Count >= tempNode.availableActions)
                {
                    //DO THE SELECTION
                    maxValue = 0.0;
                    bestNode = 0;

                    for (int i = 0; i < tempNode.childNodes.Count; i++)
                    {
                        double exploreTerm = Math.Sqrt(Math.Log(tempNode.visitCount) / Convert.ToDouble(tempNode.childNodes[i].visitCount));
                        double tempValue = Convert.ToDouble(tempNode.childNodes[i].value) + (Convert.ToDouble(BOARDSIZE) * exploreTerm);

                        if (tempValue > maxValue)
                        {
                            maxValue = tempValue;
                            bestNode = i;
                        }

                    }

                    tempNode = tempNode.childNodes[bestNode];
                }

                //IF COUNT < ACTIONS THEN TRY ANOTHER ACTION
                node tempN = playNextAction(tempNode, tempNode.nextPlayer);

                if (tempN != null)
                {
                    //SIMULATE THE ACTION AND BACKTRACK VALUES
                    if (simulateNode(tempN, currentTurn))
                    {
                        //IF WIN THEN BACKTRACK WINCOUNT
                        tempN.value += 1;

                        tempNode.value += 1;

                        while (tempNode.parentNode != null)
                        {
                            tempNode = tempNode.parentNode;
                            tempNode.value += 1;
                        }

                    }

                    tempNode.childNodes.Add(tempN);
                }
            }

            //ONCE THE TIME IS DONE CHECK THE BEST MOVE FROM ROOT NODE BASED ON THE VALUE
            tempNode = ROOTNODE;

            if (tempNode.childNodes.Count > 0)
            {
                int max = 0;
                bestNode = 0;
                List<int> equalNodes = new List<int>();

                for (int i = 0; i < tempNode.childNodes.Count; i++)
                {
                    int tempValue = tempNode.childNodes[i].value;

                    if (tempValue == max)
                    {
                        equalNodes.Add(i);
                        continue;
                    }

                    if (tempValue > max)
                    {
                        max = tempValue;
                        bestNode = i;
                        equalNodes.Clear();
                    }

                }

                if (equalNodes.Count == 0)
                {
                    nextMoves.Add(tempNode.childNodes[bestNode].moveX);
                    nextMoves.Add(tempNode.childNodes[bestNode].moveY);
                }
                else
                {
                    int possibleMove = pickMove.Next(equalNodes.Count);
                    nextMoves.Add(tempNode.childNodes[equalNodes[possibleMove]].moveX);
                    nextMoves.Add(tempNode.childNodes[equalNodes[possibleMove]].moveY);
                }

                return nextMoves;
            }
            else 
            {
                nextMoves.Add(5000);
                return nextMoves;
            }
        }


        #region "PRIVATE FUNCTIONS"

        private node playNextAction(node currentNode, int currentTurn)
        {
            node tempNode = new node();
            int actionCount = 0;

            tempNode.nodeNumber = currentNodeNumber;
            tempNode.parentNode = currentNode;

            currentNodeNumber += 1;

            tempNode.boardConfig = new int[BOARDSIZE, BOARDSIZE];
            copyArray(currentNode.boardConfig, tempNode.boardConfig);

            bool played = false;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if ((currentNode.boardConfig[i, j] == 0) && (isLiberalMove(i, j, currentNode.boardConfig, currentTurn)))
                    {
                        if (actionCount > currentNode.childNodes.Count - 1)
                        {
                            tempNode.boardConfig[i, j] = currentTurn;
                            tempNode.moveX = i;
                            tempNode.moveY = j;
                            played = true;
                            break;
                        }

                        actionCount += 1;
                    }

                }

                if (played)
                    break;
            }

            if (played)
            {
                copyArray(boardOps.captureCoins(tempNode.boardConfig, currentTurn), tempNode.boardConfig);

                tempNode.availableActions = getAvailableActions(tempNode);

                if (currentTurn == 1)
                {
                    tempNode.nextPlayer = 2;
                }
                else
                {
                    tempNode.nextPlayer = 1;
                }
                return tempNode;
            }
            else
            {
                return null;
            }

            
        }

        private int getAvailableActions(node currentNode)
        {
            int tempCount = 0;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if (currentNode.boardConfig[i, j] == 0)
                    {
                        tempCount += 1;
                    }
                }

            }

            return tempCount;
        }

        private bool simulateNode(node currentNode, int currentPlay)
        {
            //INCREMENT VISIT COUNT FROM CURREN NODE TILL PARENT
            node tempNode = currentNode;
            tempNode.visitCount += 1;

            while (tempNode.parentNode != null)
            {
                tempNode = tempNode.parentNode;
                tempNode.visitCount += 1;
            }

            int[,] tempBoard = new int[BOARDSIZE, BOARDSIZE];
            int tempTurn = currentPlay;
            int X = 0;
            int Y = 0;
            int simulationCount = 0;

            copyArray(currentNode.boardConfig, tempBoard);

            while (simulationCount < (BOARDSIZE * BOARDSIZE))
            {
                simulationCount += 1;
                int randCount = 0;
                bool randHit = false;

                while (true)
                {
                    X = moveGenX.Next(BOARDSIZE);
                    Y = moveGenY.Next(BOARDSIZE);
                    randCount += 1;

                    if ((tempBoard[X, Y] != 0) && (isLiberalMove(X, Y, tempBoard, tempTurn)))
                    {
                        break;
                    }

                    if (randCount >= BOARDSIZE)
                    {
                        randHit = true;
                        break;
                    }
                }

                tempBoard[X, Y] = tempTurn;

                copyArray(boardOps.captureCoins(tempBoard, tempTurn), tempBoard);

                //SWITCH PLAY
                if (tempTurn == 1)
                    tempTurn = 2;
                else
                    tempTurn = 1;
            }

            if (winningCoin(tempBoard) == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int winningCoin(int[,] currentBoard)
        {
            int blackCount = 0;
            int whiteCount = 0;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if (currentBoard[i, j] == 1)
                    {
                        whiteCount += 1;
                    }

                    if (currentBoard[i, j] == 2)
                    {
                        blackCount += 1;
                    }
                }
            }

            if (whiteCount >= blackCount)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }


        int[,] createRandomPlay(int[,] currentBoard, int toPlay)
        {

            int[,] tempBoard = new int[BOARDSIZE, BOARDSIZE];
            int X = 0;
            int Y = 0;

            copyArray(currentBoard, tempBoard);



            tempBoard[X, Y] = toPlay;

            return tempBoard;
        }

        private bool expandRootNode()
        {
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if (ROOTNODE.boardConfig[i, j] == 0)
                    {
                        if (isLiberalMove(i, j, ROOTNODE.boardConfig, currentTurn))
                        {
                            node tempNode = new node();
                            tempNode.nodeNumber = currentNodeNumber;
                            tempNode.parentNode = ROOTNODE;
                            tempNode.boardConfig = new int[BOARDSIZE, BOARDSIZE];

                            currentNodeNumber += 1;

                            copyArray(ROOTNODE.boardConfig, tempNode.boardConfig);

                            tempNode.boardConfig[i, j] = currentTurn;

                            //CAPTURE COINS AFTER COIN INSERTION
                            copyArray(boardOps.captureCoins(tempNode.boardConfig, currentTurn), tempNode.boardConfig);

                            ROOTNODE.childNodes.Add(tempNode);
                        }
                    }
                }
            }

            if (ROOTNODE.childNodes.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool isLiberalMove(int X, int Y, int[,] currentBoard, int currentPlay)
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

        private void copyArray(int[,] source, int[,] dest)
        {
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    dest[i, j] = source[i, j];
                }
            }
        }

        private void SwitchToNextTurn()
        {
            if (currentTurn == 1)
            {
                currentTurn = 2;
            }
            else
            {
                currentTurn = 1;
            }

        }
        #endregion
    }
}
