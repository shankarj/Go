using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

namespace GoGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int boardSize = 0;
        int currentTurn = 1;

        bool gameStarted = false;
        bool workComplete = false;

        int komi = 0;
        double secondsForMove = 0.0;

        bool computerPass = false;
        bool playerPass = false;

        int[,] boardConfiguration;
        List<int> nextMove = new List<int>();

        Stopwatch moveTime = new Stopwatch();

        boardOperations boardOps;

        Dictionary<string, FrameworkElement> tilesDictionary = new Dictionary<string, FrameworkElement>();

        BackgroundWorker computerWorker = new System.ComponentModel.BackgroundWorker();

        List<cutItem> tempCuts = new List<cutItem>();
        string[] tempString;

        int nodesGenerated = 0;

        StreamWriter nodesGenOutput = new StreamWriter(Environment.CurrentDirectory + "\\outp.txt");

        public MainWindow()
        {
            InitializeComponent();

            computerWorker.WorkerReportsProgress = true;
            computerWorker.WorkerSupportsCancellation = true;
            computerWorker.DoWork += computerWorker_DoWork;
            computerWorker.RunWorkerCompleted += computerWorker_RunWorkerCompleted;

        }



        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            boardSize = Convert.ToInt16(txtBoardSize.Text);
            int tileCount = 0;

            for (int i = 0; i < boardSize; i++)
            {
                StackPanel tempPanel = new StackPanel();
                tempPanel.Orientation = Orientation.Horizontal;

                for (int j = 0; j < boardSize; j++)
                {
                    Button tempBtn = new Button();
                    tempBtn.Width = 50;
                    tempBtn.Height = 50;
                    tempBtn.Name = "b_" + i + "_" + j;

                    tempBtn.Background = Brushes.BurlyWood;

                    tempBtn.Click += tempBtn_Click;

                    tilesDictionary.Add(i + "_" + j, tempBtn);

                    tempPanel.Children.Add(tempBtn);

                    tileCount += 1;
                }

                stackBoard.Children.Add(tempPanel);
            }

            //INITIALIZE BOARD
            boardOps = new boardOperations(boardSize);
            boardConfiguration = new int[boardSize, boardSize];
            boardConfiguration = boardOps.initializeBoard(boardConfiguration);
        }

        void tempBtn_Click(object sender, RoutedEventArgs e)
        {
            btnPass.Focus();

            if (!(playerPass))
            {
                if (gameStarted)
                {
                    if (((Button)sender).Background == Brushes.BurlyWood)
                    {
                        tempString = ((Button)sender).Name.Split(new Char[] { '_' });


                        if (currentTurn == 1)
                        {
                            ((Button)sender).Background = Brushes.White;
                            boardConfiguration[Convert.ToInt16(tempString[1]), Convert.ToInt16(tempString[2])] = currentTurn;

                            //UPDATE BOARD AFTER CAPTURE
                            boardConfiguration = boardOps.captureCoins(boardConfiguration, currentTurn);

                            //UPDATE STATS
                            lblTime.Content = "LAST MOVE TIME : " + moveTime.Elapsed.Seconds + " SEC.";
                            lblLastMove.Content = "LAST MOVE : " + tempString[1] + "," + tempString[2];

                            tempCuts = boardOps.returnCuts();

                            //UPDATE GUI
                            if (tempCuts != null)
                            {
                                for (int loopCount = 0; loopCount < tempCuts.Count; loopCount++)
                                {
                                    ((Button)tilesDictionary[tempCuts[loopCount].X + "_" + tempCuts[loopCount].Y]).Background = Brushes.BurlyWood;
                                }
                            }

                            //CHANGE TURN
                            lblTurn.Content = "CURRENT TURN : COMPUTER (THINKING...)";
                            currentTurn = 2;

                            //COMPUTER PLAYS
                            MCTS computerPlay = new MCTS(boardSize, boardConfiguration);
                            computerWorker.RunWorkerAsync(computerPlay);
                        }


                    }
                }
            }
            else
            {
                //CHANGE TURN
                lblTurn.Content = "CURRENT TURN : COMPUTER (THINKING...)";
                currentTurn = 2;

                //COMPUTER PLAYS
                MCTS computerPlay = new MCTS(boardSize, boardConfiguration);
                computerWorker.RunWorkerAsync(computerPlay);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            gameStarted = true;


            secondsForMove = Convert.ToDouble(txtTime.Text);
            komi = Convert.ToInt16(txtKomi.Text);


            lblTurn.Content = "CURRENT TURN : YOU";
            moveTime.Start();
        }

        void computerWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            nextMove = (List<int>)e.Result;

            computerPass = false;


            if (!(nextMove[0] == 5000))
            {
                boardConfiguration[Convert.ToInt16(nextMove[0]), Convert.ToInt16(nextMove[1])] = currentTurn;

                //UPDATE MOVE IN BOARD
                ((Button)tilesDictionary[nextMove[0] + "_" + nextMove[1]]).Background = Brushes.Black;

                ////UPDATE BOARD AFTER CAPTURE
                boardConfiguration = boardOps.captureCoins(boardConfiguration, currentTurn);

                //UPDATE STATS
                lblTime.Content = "LAST MOVE TIME : " + moveTime.Elapsed.Seconds + " SEC.";
                lblLastMove.Content = "LAST MOVE : " + nextMove[0] + "," + nextMove[1];

                tempCuts = boardOps.returnCuts();

                //UPDATE GUI
                if (tempCuts != null)
                {
                    for (int loopCount = 0; loopCount < tempCuts.Count; loopCount++)
                    {
                        ((Button)tilesDictionary[tempCuts[loopCount].X + "_" + tempCuts[loopCount].Y]).Background = Brushes.BurlyWood;
                    }
                }
            }
            else
            {
                lblTime.Content = "LAST MOVE TIME : " + moveTime.Elapsed.Seconds + " SEC.";
                lblLastMove.Content = "LAST MOVE : PASS";
                computerPass = true;
            }

            //CHANGE TURN
            lblTurn.Content = "CURRENT TURN : YOU";
            currentTurn = 1;

            if (tempCuts != null)
            {
                tempCuts.Clear();
            }

            moveTime.Restart();
        }

        void computerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MCTS tempMCTS = (MCTS)e.Argument;
            
            e.Result = tempMCTS.getNextMove(moveTime, secondsForMove);
            nodesGenerated = tempMCTS.currentNodeNumber;

        }

        private void btnPass_Click(object sender, RoutedEventArgs e)
        {
            int blackCount = 0;
            int whiteCount = 0;

            if (computerPass)
            {
                MessageBox.Show("Looks like we both passed. Ending the game");

                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        if (boardConfiguration[i, j] == 1)
                        {
                            whiteCount += 1;
                        }

                        if (boardConfiguration[i, j] == 2)
                        {
                            blackCount += 1;
                        }
                    }
                }

                blackCount += komi;

                if (whiteCount >= blackCount)
                {
                    lblTurn.Content = "WHITE WINS";
                }
                else
                {
                    lblTurn.Content = "BLACK WINS";
                }

            }
            else
            {
                playerPass = true;
                tempBtn_Click(null, null);
            }


        }
    }
}
