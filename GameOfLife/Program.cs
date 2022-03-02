//#define USING_DIVISION_SOLUTION
#define USING_DINAMIC_SOLUTION
using System;
using System.Threading;

namespace GameOfLife
{
    class Program
    {
        public static int Rows = 64;
        public static int Columns = 64;

        public static char aliveChar = 'O';
        public static char deathChar = 'X';
        private static bool quit = false;

        private static string[] fileLines;
        public static int stepTime = 100; // In my opinion 100 is very fast
        static void Main(string[] args)
        {

            // randomly initialize our grid with the difined Rows and Columns, for doing that you must to coment the line of bool[,] lifeGameMatrix = readFile();

            /*bool[,] lifeGameMatrix = new bool[Rows,Columns];
            Random random = new Random();
             for (var row = 0; row < Rows; row++)
            {
                for (var column = 0; column < Columns; column++)
                {
                    int num = (int)random.Next(0, 2);
                    if (num!=0)
                        lifeGameMatrix[row, column] = true;
                    else
                        lifeGameMatrix[row, column] = false;
                }
            }*/

            //First of all read the file and create the matrix
            bool[,] lifeGameMatrix = readFile();

            //Set variables in MultiThreadTaskManager as rows and columns
            MultiThreadTaskManager.setVariables(Rows, Columns);

            //Print first state of the game
            Print(lifeGameMatrix);

            //Start the game by a Thread
            Thread gameOfLifeThread = new Thread(iterateLifeGameThread);
            gameOfLifeThread.Start(lifeGameMatrix);

            //Stop the game if you press Q
            while (!quit)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                quit = (keyInfo.Key == ConsoleKey.Q);
                MultiThreadTaskManager.exitTask();
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Finishing program");
        }

        private static bool[,] readFile()
        {
            //File propiedades.txt is inside the project, if you want to use a bigger matrix, change propiedades.txt to test.txt or use yours adding it to GameOfLife\GameOfLife\bin\Debug\netcoreapp3.1\Data
            fileLines = System.IO.File.ReadAllLines(Environment.CurrentDirectory+@"\Data\propiedades.txt");
            
            //Separate the first line by , to obtain the rows and columns
            string[] aux = fileLines[0].Split(',');
            char[] charAux;
            Rows = int.Parse(aux[0]);
            Columns = int.Parse(aux[1]);
            //initialize the matrix
            bool[,] matrix = new bool[Rows, Columns];
            int pointer = 1;

            for (int row = 0; row < fileLines.Length - 1; row++)
            {
                //Get the lines of the text and divide it by individual chars
                charAux = fileLines[pointer].ToCharArray();
                //If the user didnt complete the line with the same number of chars as columns we will complete the diference with death cells
                if (charAux.Length < Columns)
                {
                    
                    for (int i = 0; i < charAux.Length; i++)
                    {
                        //Check alive cells or death cells of txt
                        if (charAux[i] != ' ')
                        {
                            matrix[row, i] = true;
                        }
                        else
                        {
                            matrix[row, i] = false;
                        }
                    }
                    //rest of columns
                    for (int i = 0; i < Columns - charAux.Length; i++)
                    {
                        matrix[row, i + charAux.Length] = false;
                    }
                }
                else
                {
                    for (int column = 0; column < Columns; column++)
                    {
                        if (charAux[column] != ' ')
                        {
                            matrix[row, column] = true;
                        }
                        else
                        {
                            matrix[row, column] = false;
                        }
                    }
                }
                pointer++;
            }
            //If the user didnt complete the line with the same number of chars as rows we will complete the diference with death cells
            if (fileLines.Length - 1 < Rows)
            {
                for (int row = fileLines.Length - 1; row < Rows; row++)
                {
                    for (int col = 0; col < Columns; col++)
                    {
                        matrix[row, col] = false;
                    }
                }
            }
            return matrix;
        }
#if USING_DIVISION_SOLUTION
        private static void iterateLifeGameThread(object lifeGameMatrix)
        {
            while (!quit)
            {
                //Pass the matrix to the MultiThreadTaskManager for make a copy
                MultiThreadTaskManager.setMatrix((bool[,])lifeGameMatrix);

                //Start the function of dividing the matrix
                MultiThreadTaskManager.addTask();
                while (!quit)
                {
                    if (MultiThreadTaskManager.getNumOfFinishedThreads() == MultiThreadTaskManager.MaxNumberOfThreads)
                    {
                        break;
                    }
                }
                if (!quit)
                {
                //Change the matrix for the modify Matrix and print it
                    lifeGameMatrix = MultiThreadTaskManager.getMatrix();
                    Print((bool[,])lifeGameMatrix);
                }
                Thread.Sleep(stepTime);
            }
        }
#else
        private static void iterateLifeGameThread(object lifeGameMatrix)
        {
            while (!quit)
            {
                //Pass the matrix to the MultiThreadTaskManager for make a copy
                MultiThreadTaskManager.setMatrix((bool[,])lifeGameMatrix);

                //Each cell is sending a request to the MultiThreadTaskManager for making an action
                for (int row = 0; row < Rows; row++)
                {
                    for (int column = 0; column < Columns; column++)
                    {
                        MultiThreadTaskManager.addTask(row * Columns + column);
                    }
                }
                while (!quit)
                {
                    if (MultiThreadTaskManager.getNumOfFinishedThreads() == Rows * Columns)
                    {
                        break;
                    }
                }
                if (!quit)
                {
                    //Change the matrix for the modify Matrix and print it
                    lifeGameMatrix = MultiThreadTaskManager.getMatrix();
                    Print((bool[,])lifeGameMatrix);
                }
                Thread.Sleep(stepTime);
            }
        }
#endif
        private static void Print(bool[,] matrix)
        {
            Console.Clear();
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    bool cell = matrix[row, column];
                    if (cell == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(aliveChar);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(deathChar);
                    }
                }
                Console.WriteLine();
            }
        }


    }
}